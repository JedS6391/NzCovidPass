using Dahomey.Cbor.ObjectModel;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Extension methods for working with CBOR data.
    /// </summary>
    internal static class CborValueExtensions
    {
        /// <summary>
        /// Gets the bytes of the provided <see cref="CborValue" />.
        /// </summary>
        /// <param name="cborValue">The CBOR value.</param>
        /// <returns>The bytes of the CBOR value.</returns>
        public static ReadOnlyMemory<byte> GetValueBytes(this CborValue cborValue)
        {
            ArgumentNullException.ThrowIfNull(cborValue);

            var bytes = cborValue.Value<ReadOnlyMemory<byte>>();

            return bytes;
        }
    }
}
