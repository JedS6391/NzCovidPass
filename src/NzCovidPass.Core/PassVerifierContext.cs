using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core
{
    /// <summary>
    /// Encapsulates details of the verification process.
    /// </summary>
    public class PassVerifierContext : Context
    {
        private CborWebToken? _token;

        /// <summary>
        /// Gets the token that was verified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will only be set when <see cref="HasSucceeded" /> is <see langword="true" />.
        /// </para>
        /// <para>
        /// Attempting to access when <see cref="HasSucceeded" /> is <see langword="false" /> will throw an <see cref="InvalidOperationException" />.
        /// </para>
        /// </remarks>
        public CborWebToken Token => (HasSucceeded && _token != null) ?
            _token :
            throw new InvalidOperationException("Token has not been set.");

        /// <summary>
        /// Gets the public COVID pass contained in the token that was verified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will only be available when <see cref="HasSucceeded" /> is <see langword="true" />.
        /// </para>
        /// <para>
        /// Attempting to access when <see cref="HasSucceeded" /> is <see langword="false" /> will throw an <see cref="InvalidOperationException" />.
        /// </para>
        /// </remarks>
        public PublicCovidPass Credentials => Token.Credentials;

        /// <summary>
        /// Marks the context as succeeded for <paramref name="token" />.
        /// </summary>
        /// <param name="token">The verified token.</param>
        public void Succeed(CborWebToken token)
        {
            base.Succeed();

            _token = token;
        }

        public static FailureReason InvalidPassComponents => new(nameof(InvalidPassComponents), "Invalid pass components.");
        public static FailureReason InvalidPassPayload => new(nameof(InvalidPassPayload), "Invalid pass payload.");
        public static FailureReason PrefixValidationFailed => new(nameof(PrefixValidationFailed), "Prefix validation failed.");
        public static FailureReason VersionValidationFailed => new(nameof(VersionValidationFailed), "Version validation failed.");
        public static FailureReason TokenReadFailed => new(nameof(TokenReadFailed), "Token read failed.");
        public static FailureReason TokenValidationFailed => new(nameof(TokenValidationFailed), "Token validation failed.");
    }
}
