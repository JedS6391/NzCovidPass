using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Shared;
using NzCovidPass.Core.Verification;

namespace NzCovidPass.Core
{
    public class PassVerifier
    {
        private readonly ILogger<PassVerifier> _logger;
        private readonly PassVerifierOptions _verifierOptions;
        private readonly ICborWebTokenReader _tokenReader;
        private readonly IVerificationKeyProvider _verificationKeyProvider;

        public PassVerifier(
            ILogger<PassVerifier> logger,
            IOptions<PassVerifierOptions> verifierOptionsAccessor,
            ICborWebTokenReader tokenReader,
            IVerificationKeyProvider verificationKeyProvider)
        {
            _logger = Requires.NotNull(logger);
            _verifierOptions = Requires.NotNull(verifierOptionsAccessor).Value;
            _tokenReader = Requires.NotNull(tokenReader);
            _verificationKeyProvider = Requires.NotNull(verificationKeyProvider);
        }

        public async Task<PassVerifierResult> VerifyAsync(string passPayload)
        {
            ArgumentNullException.ThrowIfNull(passPayload);

            var result = new PassVerifierResult();

            _logger.LogDebug("Verifying pass payload '{Payload}'", passPayload);

            var passComponents = passPayload.Split('/', 3, StringSplitOptions.None);

            if (!string.Equals(passComponents[0], _verifierOptions.Prefix, StringComparison.Ordinal))
            {
                _logger.LogError("Prefix validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Prefix, passComponents[0]);

                result.Fail(PassVerifierResult.PrefixValidationFailed);

                return result;
            }

            if (!int.TryParse(passComponents[1], out var version) || version != _verifierOptions.Version)
            {
                _logger.LogError("Version validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Version, passComponents[1]);

                result.Fail(PassVerifierResult.VersionValidationFailed);

                return result;               
            }

            var base32Payload = AddBase32Padding(passComponents[2]);

            _logger.LogDebug("Decoding base-32 payload '{Payload}'", base32Payload);

            var decodedPayloadBytes = Base32.ToBytes(base32Payload);

            _logger.LogDebug("Decoded base-32 payload bytes (hex) '{Payload}'", Convert.ToHexString(decodedPayloadBytes));

            if (!_tokenReader.TryReadToken(decodedPayloadBytes, out var token))
            {
                _logger.LogError("Token read failed [Decoded Payload Bytes (hex) = '{Payload}']", Convert.ToHexString(decodedPayloadBytes));

                result.Fail(PassVerifierResult.TokenReadFailed);

                return result;                  
            };

            if (!_verifierOptions.ValidIssuers.Contains(token.Issuer))
            {
                _logger.LogError("Issuer validation failed [Expected = '{{ {Expected} }}', Actual = '{Actual}']", string.Join(", ", _verifierOptions.ValidIssuers), token.Issuer);

                result.Fail(PassVerifierResult.IssuerValidationFailed);

                return result;                 
            }

            var verificationKey = _verificationKeyProvider.GetKeyAsync(token.Issuer, token.KeyId);

            result.Succeed(token);

            return result;            
        } 

        private string AddBase32Padding(string base32Payload)
        {
            var unpaddedLength = base32Payload.Length % 8;

            if (unpaddedLength != 0) 
            {
                base32Payload += new string('=', count: 8 - unpaddedLength);
            }

            return base32Payload;
        }    
    }
}