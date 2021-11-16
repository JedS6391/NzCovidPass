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

            var token = context.Token;

            _logger.LogDebug("Validating token payload");

            ValidatePayload(context);

            if (context.HasFailed)
            {
                _logger.LogError("Token payload is not valid");

                return;
            }

            _logger.LogDebug("Validating token signature");

            var verificationKey = await GetVerificationKeyAsync(token).ConfigureAwait(false);

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

            token.SigningKey = verificationKey;

            context.Succeed();

            return;
        }

        private void ValidatePayload(CwtSecurityTokenValidatorContext context)
        {
            ValidateRequiredClaims(context);
            ValidateAlgorithm(context);
            ValidateIssuer(context);
            ValidateLifetime(context);
        }

        private void ValidateRequiredClaims(CwtSecurityTokenValidatorContext context)
        {
            var token = context.Token;

            if (string.IsNullOrEmpty(token.KeyId))
            {
                _logger.LogError("Key ID validation failed");

                context.Fail(CwtSecurityTokenValidatorContext.KeyIdValidationFailed);
            }

            if (string.IsNullOrEmpty(token.Id))
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

                context.Fail(CwtSecurityTokenValidatorContext.AlgorithmValidationFailed);
            }
        }

        private void ValidateIssuer(CwtSecurityTokenValidatorContext context)
        {
            var token = context.Token;

            if (!_verifierOptions.ValidIssuers.Contains(token.Issuer))
            {
                _logger.LogError("Issuer validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidIssuers), token.Issuer);

                context.Fail(CwtSecurityTokenValidatorContext.IssuerValidationFailed);
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

                context.Fail(CwtSecurityTokenValidatorContext.LifetimeValidationFailed);
            }

            if (token.Expiry < utcNow)
            {
                _logger.LogError("Lifetime validation failed [Expiry ({Expiry}) > UtcNow ({UtcNow})]", token.Expiry, utcNow);

                context.Fail(CwtSecurityTokenValidatorContext.LifetimeValidationFailed);
            }
        }

        private void ValidateSignature(CwtSecurityTokenValidatorContext context, SecurityKey key)
        {
            var token = context.Token;
            var algorithm = token.Algorithm;
            var signature = token.SignatureBytes;

            var signedBytes = GetSignedBytes(token);

            var cryptoProviderFactory = key.CryptoProviderFactory;

            if (!cryptoProviderFactory.IsSupportedAlgorithm(algorithm, key))
            {
                _logger.LogError("Signature validation failed [Algorithm '{Algorithm}' is not supported for key type '{KeyType}']", algorithm, key.GetType().Name);

                context.Fail(CwtSecurityTokenValidatorContext.SignatureValidationFailed);
            }

            var signatureProvider = cryptoProviderFactory.CreateForVerifying(key, algorithm);

            try
            {
                if (!signatureProvider.Verify(signedBytes, signature))
                {
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
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to retrieve verification key.");

                return null;
            }
        }

        private static byte[] GetSignedBytes(CwtSecurityToken token)
        {
            // https://datatracker.ietf.org/doc/html/rfc8152#section-4.4
            // Note this process assumes a COSE_Sign1 structure, which NZ Covid passes will be.
            var b = new ArrayBufferWriter<byte>();
            var w = new CborWriter(b);

            w.WriteBeginArray(4);

            w.WriteString("Signature1");
            w.WriteByteString(token.HeaderBytes);
            w.WriteByteString(Array.Empty<byte>());
            w.WriteByteString(token.PayloadBytes);

            w.WriteEndArray(4);

            return b.WrittenMemory.ToArray();
        }
    }
}
