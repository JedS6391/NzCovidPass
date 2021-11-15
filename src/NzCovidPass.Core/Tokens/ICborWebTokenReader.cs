namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Defines the ability to read <see cref="CborWebToken" /> instances.
    /// </summary>
    public interface ICborWebTokenReader
    {
        /// <summary>
        /// Attempts to read a <see cref="CborWebToken" /> instance from the payload provided in <paramref name="context" />.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The base-32 payload to read from is provided in <see cref="CborWebTokenReaderContext.Payload" />.
        /// </para>
        /// <para>
        /// The provided <see cref="CborWebTokenReaderContext" /> will be updated to indicate whether reading succeeded or not.
        /// </para>
        /// </remarks>
        /// <param name="context">A context to manage details of the read process.</param>
        void ReadToken(CborWebTokenReaderContext context);
    }
}
