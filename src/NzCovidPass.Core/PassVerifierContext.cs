using NzCovidPass.Core.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core
{
    /// <summary>
    /// Encapsulates details of the verification process.
    /// </summary>
    public class PassVerifierContext : ValidationContext
    {
        private CborWebToken? _token;

        /// <summary>
        /// Gets the token that was verified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will only be set when <see cref="ValidationContext.HasSucceeded" /> is <see langword="true" />.
        /// </para>
        /// <para>
        /// Attempting to access when <see cref="ValidationContext.HasSucceeded" /> is <see langword="false" /> will throw an <see cref="InvalidOperationException" />.
        /// </para>
        /// </remarks>
        public CborWebToken Token => (HasSucceeded && _token is not null) ?
            _token :
            throw new InvalidOperationException("Token has not been set.");

        /// <summary>
        /// Gets the public COVID pass contained in the token that was verified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will only be available when <see cref="ValidationContext.HasSucceeded" /> is <see langword="true" />.
        /// </para>
        /// <para>
        /// Attempting to access when <see cref="ValidationContext.HasSucceeded" /> is <see langword="false" /> will throw an <see cref="InvalidOperationException" />.
        /// </para>
        /// </remarks>
        public PublicCovidPass Credentials => Token.Credentials;

        /// <summary>
        /// Indicates that validation has succeeded for this context, with the provided <paramref name="token" />.
        /// </summary>
        /// <param name="token">The verified token.</param>
        public void Succeed(CborWebToken token)
        {
            base.Succeed();

            _token = token;
        }

        /// <summary>
        /// Invalid pass components failure reason.
        /// </summary>
        public static FailureReason InvalidPassComponents => new(nameof(InvalidPassComponents), "Invalid pass components.");

        /// <summary>
        /// Invalid pass payload failure reason.
        /// </summary>
        public static FailureReason InvalidPassPayload => new(nameof(InvalidPassPayload), "Invalid pass payload.");

        /// <summary>
        /// Failed prefix validation failure reason.
        /// </summary>
        public static FailureReason PrefixValidationFailed => new(nameof(PrefixValidationFailed), "Prefix validation failed.");

        /// <summary>
        /// Failed version validation failure reason.
        /// </summary>
        public static FailureReason VersionValidationFailed => new(nameof(VersionValidationFailed), "Version validation failed.");

        /// <summary>
        /// Failed token read failure reason.
        /// </summary>
        public static FailureReason TokenReadFailed => new(nameof(TokenReadFailed), "Token read failed.");

        /// <summary>
        /// Failed token validation failure reason.
        /// </summary>
        public static FailureReason TokenValidationFailed => new(nameof(TokenValidationFailed), "Token validation failed.");
    }
}
