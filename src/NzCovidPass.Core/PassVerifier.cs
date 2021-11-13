using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core
{
    /// <summary>
    /// Provides the ability to verify New Zealand COVID Pass payloads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The New Zealand COVID Pass is a cryptographically signed document which can be represented in the form
    /// of a QR Code that enables an individual to express proof of having met certain health policy requirements
    /// in regards to COVID-19 such as being vaccinated against the virus.
    /// </para>
    /// <para>
    /// For more details, see <see href="https://nzcp.covid19.health.nz" />.
    /// </para>
    /// </remarks>
    public class PassVerifier
    {
        private readonly ILogger<PassVerifier> _logger;
        private readonly PassVerifierOptions _verifierOptions;
        private readonly ICborWebTokenReader _tokenReader;
        private readonly ICborWebTokenValidator _tokenValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassVerifier"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        /// <param name="verifierOptionsAccessor">An accessor for <see cref="PassVerifierOptions" /> instances.</param>
        /// <param name="tokenReader">An <see cref="ICborWebTokenReader" /> instance used to read CBOR Web Token (CWT) data.</param>
        /// <param name="tokenValidator">An <see cref="ICborWebTokenValidator" /> instance used to validate CBOR Web Token (CWT) data.</param>
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

        /// <summary>
        /// Verifies the contents of <paramref name="passPayload" /> according to the New Zealand Covid Pass specification.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="passPayload" /> is expected to be the raw payload obtained from a QR code or other form, following the format <c>NZCP:/{version-identifier}/{base32-encoded-CWT}</c>.
        /// </para>
        /// <para>
        /// The resulting <see cref="PassVerifierContext" /> instance can be inspected to determine whether the payload could be verified or not.
        /// </para>
        /// </remarks>
        /// <param name="passPayload">The raw pass payload.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the details of the verification process.</returns>
        public async Task<PassVerifierContext> VerifyAsync(string passPayload)
        {
            ArgumentNullException.ThrowIfNull(passPayload);

            var context = new PassVerifierContext();

            _logger.LogDebug("Verifying pass payload '{Payload}'", passPayload);

            // Check the payload adheres to expected format.
            var passComponents = passPayload.Split('/', 3, StringSplitOptions.None);

            if (passComponents.Length != 3)
            {
                _logger.LogError("Expected 3 components separated by '/' in pass payload");

                context.Fail(PassVerifierContext.InvalidPassComponents);

                return context;
            }

            // Validate the pass components
            ValidatePassComponents(context, passComponents);

            // Read the token to validate its contents and signature
            var payload = passComponents[2];

            if (!_tokenReader.TryReadToken(payload, out var token) || token == null)
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

        private void ValidatePassComponents(PassVerifierContext context, string[] passComponents)
        {
            var prefix = passComponents[0];
            var version = passComponents[1];
            var payload = passComponents[2];

            if (!string.Equals(prefix, _verifierOptions.Prefix, StringComparison.Ordinal))
            {
                _logger.LogError("Prefix validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Prefix, prefix);

                context.Fail(PassVerifierContext.PrefixValidationFailed);
            }

            if (!int.TryParse(version, out var versionNumber) || versionNumber != _verifierOptions.Version)
            {
                _logger.LogError("Version validation failed [Expected = '{Expected}', Actual = '{Actual}']", _verifierOptions.Version, version);

                context.Fail(PassVerifierContext.VersionValidationFailed);
            }

            if (string.IsNullOrEmpty(payload))
            {
                _logger.LogError("Invalid pass payload.");

                context.Fail(PassVerifierContext.InvalidPassPayload);
            }
        }
    }
}
