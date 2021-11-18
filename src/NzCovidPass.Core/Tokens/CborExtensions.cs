using Dahomey.Cbor.ObjectModel;
using Dahomey.Cbor.Serialization;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Extension methods for working with CBOR data.
    /// </summary>
    internal static class CborExtensions
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

        /// <summary>
        /// Writes the items in <paramref name="array" /> to the provided <see cref="CborWriter" />.
        /// </summary>
        /// <param name="writer">The CBOR writer.</param>
        /// <param name="array">The array to write.</param>
        /// <returns>The CBOR writer.</returns>
        public static CborWriter WriteArray(this CborWriter writer, object[] array)
        {
            writer.WriteBeginArray(array.Length);

            foreach (var item in array)
            {
                switch (item)
                {
                    case int i:
                        writer.WriteInt32(i);
                        break;

                    case string s:
                        writer.WriteString(s);
                        break;

                    case byte[] b:
                        writer.WriteByteString(b);
                        break;

                    default:
                        throw new NotSupportedException($"Unexpected array item type '{item.GetType().Name}'");
                }
            }

            writer.WriteEndArray(array.Length);

            return writer;
        }
    }
}
