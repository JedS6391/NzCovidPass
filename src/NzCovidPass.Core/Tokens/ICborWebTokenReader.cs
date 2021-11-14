namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Defines the ability to read <see cref="CborWebToken" /> instances.
    /// </summary>
    public interface ICborWebTokenReader
    {
        /// <summary>
        /// Attempts to read a <see cref="CborWebToken" /> instance from <paramref name="base32Payload" />.
        /// </summary>
        /// <param name="base32Payload">A CBOR web token encoded as a base-32 string.</param>
        /// <param name="token">The token read from the payload, if reading succeeds; otherwise <see langword="null" />.</param>
        /// <returns><see langword="true" /> if a token was successfully read; <see langword="false" /> otherwise.</returns>
        bool TryReadToken(string base32Payload, out CborWebToken? token);
    }
}
