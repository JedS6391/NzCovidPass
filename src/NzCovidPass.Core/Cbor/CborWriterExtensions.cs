using System.Formats.Cbor;

namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Extension methods for <see cref="CborWriter" />.
    /// </summary>
    internal static class CborWriterExtensions
    {
        public static void WriteArray(this CborWriter writer, object[] array)
        {
            writer.WriteStartArray(array.Length);

            foreach (var item in array)
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
