using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Encapsulates details of the token read process.
    /// </summary>
    public class CborWebTokenReaderContext : ValidationContext
    {
        private readonly string _base32Payload;
        private CborWebToken? _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="CborWebTokenReaderContext" /> class.
        /// </summary>
        /// <param name="base32Payload">The base-32 string to attempt to read as a CWT.</param>
        public CborWebTokenReaderContext(string base32Payload)
        {
            _base32Payload = Requires.NotNull(base32Payload);
        }

        /// <summary>
        /// Gets the base-32 payload to attempt to read as a CWT.
        /// </summary>
        public string Payload => _base32Payload;

        /// <summary>
        /// Gets the token that was read.
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
        /// Indicates that validation has succeeded for this context, with the provided <paramref name="token" />.
        /// </summary>
        /// <param name="token">The verified token.</param>
        public void Succeed(CborWebToken token)
        {
            base.Succeed();

            _token = token;
        }

        /// <summary>
        /// Invalid base-32 payload failure reason.
        /// </summary>
        public static FailureReason InvalidBase32Payload => new(nameof(InvalidBase32Payload), "Invalid base-32 payload.");

        /// <summary>
        /// Failed to decode CBOR structure failure reason.
        /// </summary>
        public static FailureReason FailedToDecodeCborStructure => new(nameof(FailedToDecodeCborStructure), "Failed to decode CBOR structure.");
    }
}
