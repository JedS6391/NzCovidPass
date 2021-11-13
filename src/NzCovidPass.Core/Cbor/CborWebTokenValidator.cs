using System.Buffers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Shared;
using NzCovidPass.Core.Verification;

namespace NzCovidPass.Core.Cbor
{
    public class CborWebTokenValidator : ICborWebTokenValidator
    {
        private readonly ILogger<CborWebTokenValidator> _logger;
        private readonly PassVerifierOptions _verifierOptions;
        private readonly IVerificationKeyProvider _verificationKeyProvider;

        public CborWebTokenValidator(
            ILogger<CborWebTokenValidator> logger,
            IOptions<PassVerifierOptions> verifierOptionsAccessor,
            IVerificationKeyProvider verificationKeyProvider)
        {
            _logger = Requires.NotNull(logger);
            _verifierOptions = Requires.NotNull(verifierOptionsAccessor).Value;
            _verificationKeyProvider = Requires.NotNull(verificationKeyProvider);
        }

        public async Task<CborWebTokenValidatorContext> ValidateTokenAsync(CborWebToken token)
        {
            ArgumentNullException.ThrowIfNull(token);

            var context = new CborWebTokenValidatorContext(token);

            _logger.LogDebug("Validating token payload");

            ValidatePayload(context);

            if (context.HasFailed)
            {
                _logger.LogError("Token payload is not valid");

                return context;
            }

            _logger.LogDebug("Validating token signature");

            var verificationKey = await GetVerificationKeyAsync(token).ConfigureAwait(false);

            if (verificationKey == null)
            {
                context.Fail(CborWebTokenValidatorContext.VerificationKeyRetrievalFailed);

                return context;
            }

            ValidateSignature(context, verificationKey);

            if (context.HasFailed)
            {
                _logger.LogError("Token signature is not valid");

                return context;
            }

            token.SigningKey = verificationKey;

            context.Succeed();

            return context;
        }

        private void ValidatePayload(CborWebTokenValidatorContext context)
        {
            ValidateRequiredClaims(context);
            ValidateAlgorithm(context);
            ValidateIssuer(context);
            ValidateLifetime(context);
        }

        private void ValidateRequiredClaims(CborWebTokenValidatorContext context)
        {
            var token = context.Token;

            if (string.IsNullOrEmpty(token.KeyId))
            {
                _logger.LogError("Key ID validation failed");

                context.Fail(CborWebTokenValidatorContext.KeyIdValidationFailed);
            }

            if (string.IsNullOrEmpty(token.Id))
            {
                _logger.LogError("Token ID validation failed");

                context.Fail(CborWebTokenValidatorContext.TokenIdValidationFailed);
            }
        }

        private void ValidateAlgorithm(CborWebTokenValidatorContext context)
        {
            var token = context.Token;

            if (!_verifierOptions.ValidAlgorithms.Contains(token.Algorithm))
            {
                _logger.LogError("Algorithm validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidAlgorithms), token.Algorithm);

                context.Fail(CborWebTokenValidatorContext.AlgorithmValidationFailed);
            }
        }

        private void ValidateIssuer(CborWebTokenValidatorContext context)
        {
            var token = context.Token;

            if (!_verifierOptions.ValidIssuers.Contains(token.Issuer))
            {
                _logger.LogError("Issuer validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidIssuers), token.Issuer);

                context.Fail(CborWebTokenValidatorContext.IssuerValidationFailed);
            }
        }

        private void ValidateLifetime(CborWebTokenValidatorContext context)
        {
            var token = context.Token;

            if (token.NotBefore > token.Expiry)
            {
                _logger.LogError("Lifetime validation failed [Not Before ({NotBefore}) > Expiry ({Expiry})]", token.NotBefore, token.Expiry);

                context.Fail(CborWebTokenValidatorContext.LifetimeValidationFailed);
            }

            var utcNow = DateTime.UtcNow;

            if (token.NotBefore > utcNow)
            {
                _logger.LogError("Lifetime validation failed [Not Before ({NotBefore}) > UtcNow ({UtcNow})]", token.NotBefore, utcNow);

                context.Fail(CborWebTokenValidatorContext.LifetimeValidationFailed);
            }

            if (token.Expiry < utcNow)
            {
                _logger.LogError("Lifetime validation failed [Expiry ({Expiry}) > UtcNow ({UtcNow})]", token.Expiry, utcNow);

                context.Fail(CborWebTokenValidatorContext.LifetimeValidationFailed);
            }
        }

        private void ValidateSignature(CborWebTokenValidatorContext context, SecurityKey key)
        {
            var token = context.Token;
            var algorithm = token.Algorithm;
            var signature = token.SignatureBytes;

            var signedBytes = GetSignedBytes(token);

            var cryptoProviderFactory = key.CryptoProviderFactory;

            if (!cryptoProviderFactory.IsSupportedAlgorithm(algorithm, key))
            {
                _logger.LogError("Signature validation failed [Algorithm '{Algorithm}' is not supported for key type '{KeyType}']", algorithm, key.GetType().Name);

                context.Fail(CborWebTokenValidatorContext.SignatureValidationFailed);
            }

            var signatureProvider = cryptoProviderFactory.CreateForVerifying(key, algorithm);

            try
            {
                if (!signatureProvider.Verify(signedBytes, signature))
                {
                    context.Fail(CborWebTokenValidatorContext.SignatureValidationFailed);
                }
            }
            finally
            {
                cryptoProviderFactory.ReleaseSignatureProvider(signatureProvider);
            }
        }

        private async Task<SecurityKey?> GetVerificationKeyAsync(CborWebToken token)
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

        private static byte[] GetSignedBytes(CborWebToken token)
        {
            // https://datatracker.ietf.org/doc/html/rfc8152#section-4.4
            var b = new ArrayBufferWriter<byte>();
            var w = new Dahomey.Cbor.Serialization.CborWriter(b);

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
