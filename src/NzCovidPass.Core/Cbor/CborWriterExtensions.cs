using System.Formats.Cbor;

namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Extension methods for <see cref="CborWriter" />.
    /// </summary>
    internal static class CborWriterExtensions
    {
        /// <summary>
        /// Writes <paramref name="collection" /> to the provided <see cref="CborWriter" />.
        /// </summary>
        /// <param name="writer">The <see cref="CborWriter" /> to write to.</param>
        /// <param name="collection">The collection to write.</param>
        public static void WriteCollection(this CborWriter writer, IReadOnlyCollection<object> collection)
        {
            writer.WriteStartArray(collection.Count);

            foreach (var item in collection)
            {
                switch (item)
                {
                    // Currently only supporting what is needed for this library.
                    case string s:
                        writer.WriteTextString(s);
                        break;

                    case byte[] b:
                        writer.WriteByteString(b);
                        break;

                    default:
                        throw new NotSupportedException($"Unexpected array item type '{item.GetType().Name}'");
                }
            }

            writer.WriteEndArray();
        }
    }
}
