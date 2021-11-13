using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core
{
    public class PassVerifier
    {
        private readonly ILogger<PassVerifier> _logger;
        private readonly PassVerifierOptions _verifierOptions;
        private readonly ICborWebTokenReader _tokenReader;
        private readonly ICborWebTokenValidator _tokenValidator;

        public PassVerifier(
            ILogger<PassVerifier> logger,
            IOptions<PassVerifierOptions> verifierOptionsAccessor,
            ICborWebTokenReader tokenReader,
            ICborWebTokenValidator tokenValidator)
        {
            _logger = Requires.NotNull(logger);
            _verifierOptions = Requires.NotNull(verifierOptionsAccessor).Value;
            _tokenReader = Requires.NotNull(tokenReader);
            _tokenValidator = Requires.NotNull(tokenValidator);
        }

        public async Task<PassVerifierContext> VerifyAsync(string passPayload)
        {
            ArgumentNullException.ThrowIfNull(passPayload);

            var context = new PassVerifierContext();

            _logger.LogDebug("Verifying pass payload '{Payload}'", passPayload);

            var passComponents = passPayload.Split('/', 3, StringSplitOptions.None);

            if (passComponents.Length != 3)
            {
                _logger.LogError("Expected 3 components separated by '/' in pass payload");

                context.Fail(PassVerifierContext.InvalidPassComponents);

                return context;
            }

            var prefix = passComponents[0];
            var version = passComponents[1];
            var payload = passComponents[2];

            if (!string.Equals(prefix, _verifierOptions.Prefix, StringComparison.Ordinal))
            {
                _logger.LogError("Prefix validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Prefix, prefix);

                context.Fail(PassVerifierContext.PrefixValidationFailed);

                return context;
            }

            if (!int.TryParse(version, out var versionNumber) || versionNumber != _verifierOptions.Version)
            {
                _logger.LogError("Version validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Version, version);

                context.Fail(PassVerifierContext.VersionValidationFailed);

                return context;               
            }

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogError("Invalid pass payload.");

                context.Fail(PassVerifierContext.InvalidPassPayload);

                return context;
            }

            if (!_tokenReader.TryReadToken(payload, out var token))
            {
                _logger.LogError("Token read failed [Payload = '{Payload}']", payload);

                context.Fail(PassVerifierContext.TokenReadFailed);

                return context;                  
            }

            var tokenValidationContext = await _tokenValidator
                .ValidateTokenAsync(token)
                .ConfigureAwait(false);

            if (tokenValidationContext.HasFailed)
            {
                context.Fail(PassVerifierContext.TokenValidationFailed);

                return context;
            }

            context.Succeed(token);

            return context;            
        }  
    }
}