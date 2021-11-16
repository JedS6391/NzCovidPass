using System.Buffers;
using Dahomey.Cbor.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Shared;
using NzCovidPass.Core.Verification;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// An <see cref="ICwtSecurityTokenValidator" /> implementation that applies the validation rules outlined in <see href="https://nzcp.covid19.health.nz/#steps-to-verify-a-new-zealand-covid-pass" />.
    /// </summary>
    public class CwtSecurityTokenValidator : ICwtSecurityTokenValidator
    {
        private readonly ILogger<CwtSecurityTokenValidator> _logger;
        private readonly PassVerifierOptions _verifierOptions;
        private readonly IVerificationKeyProvider _verificationKeyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CwtSecurityTokenValidator" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        /// <param name="verifierOptionsAccessor">An accessor for <see cref="PassVerifierOptions" /> instances.</param>
        /// <param name="verificationKeyProvider">A <see cref="IVerificationKeyProvider" /> instance used to obtain keys for signature validation.</param>
        public CwtSecurityTokenValidator(
            ILogger<CwtSecurityTokenValidator> logger,
            IOptions<PassVerifierOptions> verifierOptionsAccessor,
            IVerificationKeyProvider verificationKeyProvider)
        {
            _logger = Requires.NotNull(logger);
            _verifierOptions = Requires.NotNull(verifierOptionsAccessor).Value;
            _verificationKeyProvider = Requires.NotNull(verificationKeyProvider);
        }

        /// <inheritdoc />
        public async Task ValidateTokenAsync(CwtSecurityTokenValidatorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            ValidateHeader(context);
            ValidatePayload(context);

            if (context.HasFailed)
            {
                _logger.LogError("Token payload is not valid");

                return;
            }

            var verificationKey = await GetVerificationKeyAsync(context.Token).ConfigureAwait(false);

            if (verificationKey is null)
            {
                context.Fail(CwtSecurityTokenValidatorContext.VerificationKeyRetrievalFailed);

                return;
            }

            ValidateSignature(context, verificationKey);

            if (context.HasFailed)
            {
                _logger.LogError("Token signature is not valid");

                return;
            }

            // Now that we've validated the signature we can link the key and token.
            context.Token.SigningKey = verificationKey;

            context.Succeed();

            return;
        }

        private void ValidateHeader(CwtSecurityTokenValidatorContext context)
        {
            _logger.LogDebug("Validating token header");

            ValidateHeaderRequiredClaims(context);
            ValidateAlgorithm(context);
        }

        private void ValidatePayload(CwtSecurityTokenValidatorContext context)
        {
            _logger.LogDebug("Validating token payload");

            ValidatePayloadRequiredClaims(context);
            ValidateIssuer(context);
            ValidateLifetime(context);
        }

        private void ValidateHeaderRequiredClaims(CwtSecurityTokenValidatorContext context)
        {
            if (string.IsNullOrEmpty(context.Token.KeyId))
            {
                _logger.LogError("Key ID validation failed");

                context.Fail(CwtSecurityTokenValidatorContext.KeyIdValidationFailed);
            }
        }

        private void ValidatePayloadRequiredClaims(CwtSecurityTokenValidatorContext context)
        {
            if (string.IsNullOrEmpty(context.Token.Id))
            {
                _logger.LogError("Token ID validation failed");

                context.Fail(CwtSecurityTokenValidatorContext.TokenIdValidationFailed);
            }
        }

        private void ValidateAlgorithm(CwtSecurityTokenValidatorContext context)
        {
            var token = context.Token;

            if (!_verifierOptions.ValidAlgorithms.Contains(token.Algorithm))
            {
                _logger.LogError("Algorithm validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidAlgorithms), token.Algorithm);

                context.Fail(CwtSecurityTokenValidatorContext.AlgorithmValidationFailed(_verifierOptions.ValidAlgorithms));
            }
        }

        private void ValidateIssuer(CwtSecurityTokenValidatorContext context)
        {
            var token = context.Token;

            if (!_verifierOptions.ValidIssuers.Contains(token.Issuer))
            {
                _logger.LogError("Issuer validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidIssuers), token.Issuer);

                context.Fail(CwtSecurityTokenValidatorContext.IssuerValidationFailed(_verifierOptions.ValidIssuers));
            }
        }

        private void ValidateLifetime(CwtSecurityTokenValidatorContext context)
        {
            var token = context.Token;

            if (token.NotBefore > token.Expiry)
            {
                _logger.LogError("Lifetime validation failed [Not Before ({NotBefore}) > Expiry ({Expiry})]", token.NotBefore, token.Expiry);

                context.Fail(CwtSecurityTokenValidatorContext.LifetimeValidationFailed);
            }

            var utcNow = DateTime.UtcNow;

            if (token.NotBefore > utcNow)
            {
                _logger.LogError("Lifetime validation failed [Not Before ({NotBefore}) > UtcNow ({UtcNow})]", token.NotBefore, utcNow);

                context.Fail(CwtSecurityTokenValidatorContext.NotBeforeValidationFailed);
            }

            if (token.Expiry < utcNow)
            {
                _logger.LogError("Lifetime validation failed [Expiry ({Expiry}) > UtcNow ({UtcNow})]", token.Expiry, utcNow);

                context.Fail(CwtSecurityTokenValidatorContext.ExpiryValidationFailed);
            }
        }

        private void ValidateSignature(CwtSecurityTokenValidatorContext context, SecurityKey key)
        {
            _logger.LogDebug("Validating token signature");

            var token = context.Token;
            var algorithm = token.Algorithm;
            var signature = token.SignatureBytes;

            var signatureStructure = BuildSignatureStructure(token);

            var cryptoProviderFactory = key.CryptoProviderFactory;

            if (!cryptoProviderFactory.IsSupportedAlgorithm(algorithm, key))
            {
                _logger.LogError("Signature validation failed [Algorithm '{Algorithm}' is not supported for key type '{KeyType}']", algorithm, key.GetType().Name);

                context.Fail(CwtSecurityTokenValidatorContext.SignatureValidationFailed);
            }

            var signatureProvider = cryptoProviderFactory.CreateForVerifying(key, algorithm);

            try
            {
                if (!signatureProvider.Verify(signatureStructure, signature))
                {
                    _logger.LogError("Signature validation failed [Signature computed using {Algorithm} and {Key} is not consistent with the provided signature]", algorithm, key.GetType().Name);

                    context.Fail(CwtSecurityTokenValidatorContext.SignatureValidationFailed);
                }
            }
            finally
            {
                cryptoProviderFactory.ReleaseSignatureProvider(signatureProvider);
            }
        }

        private async Task<SecurityKey?> GetVerificationKeyAsync(CwtSecurityToken token)
        {
            try
            {
                return await _verificationKeyProvider
                    .GetKeyAsync(token.Issuer, token.KeyId)
                    .ConfigureAwait(false);
            }
            catch (VerificationKeyNotFoundException verificationKeyNotFoundException)
            {
                _logger.LogError(verificationKeyNotFoundException, "Failed to retrieve verification key.");

                return null;
            }
        }

        private static byte[] BuildSignatureStructure(CwtSecurityToken token)
        {
            // https://datatracker.ietf.org/doc/html/rfc8152#section-4.4
            // Note this process assumes a COSE_Sign1 structure, which NZ Covid passes should be.
            var b = new ArrayBufferWriter<byte>();
            var w = new CborWriter(b);

            w.WriteBeginArray(4);

            // context
            w.WriteString("Signature1");
            // body_protected
            w.WriteByteString(token.HeaderBytes);
            // external_aad
            w.WriteByteString(Array.Empty<byte>());
            // payload
            w.WriteByteString(token.PayloadBytes);

            w.WriteEndArray(4);

            return b.WrittenMemory.ToArray();
        }
    }
}
