using System.Formats.Cbor;

namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Extension methods for <see cref="CborReader" />.
    /// </summary>
    internal static class CborReaderExtensions
    {
        /// <summary>
        /// Reads a <see cref="CborObject" /> from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="CborReader" /> to read from.</param>
        /// <returns>The next CBOR encoded object.</returns>
        public static CborObject ReadObject(this CborReader reader)
        {
            var state = reader.PeekState();

            // Currently only supporting what is needed for this library.
            return state switch
            {
                CborReaderState.StartMap => reader.ReadMap(),
                CborReaderState.StartArray => reader.ReadArray(),
                CborReaderState.TextString => new CborTextString(reader.ReadTextString()),
                CborReaderState.ByteString => new CborByteString(reader.ReadByteString()),
                CborReaderState.UnsignedInteger => new CborInteger(reader.ReadInt32()),
                CborReaderState.NegativeInteger => new CborInteger(reader.ReadInt32()),
                _ => throw new NotSupportedException($"Unexpected reader state '{state}'.")
            };
        }

        /// <summary>
        /// Reads a <see cref="CborArray" /> from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="CborReader" /> to read from.</param>
        /// <returns>The next CBOR encoded array.</returns>
        public static CborArray ReadArray(this CborReader reader)
        {
            var length = reader.ReadStartArray();

            var values = length is null
                ? new List<CborObject>()
                : new List<CborObject>(length.Value);

            while (!(reader.PeekState() is CborReaderState.EndArray or CborReaderState.Finished))
            {
                values.Add(reader.ReadObject());
            }

            reader.ReadEndArray();

            return new CborArray(values);
        }

        /// <summary>
        /// Reads a <see cref="CborMap" /> from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="CborReader" /> to read from.</param>
        /// <returns>The next CBOR encoded map.</returns>
        public static CborMap ReadMap(this CborReader reader)
        {
            var count = reader.ReadStartMap();

            var values = count is null ?
                 new Dictionary<CborObject, CborObject>() :
                 new Dictionary<CborObject, CborObject>(count.Value);

            while (!(reader.PeekState() is CborReaderState.EndMap or CborReaderState.Finished))
            {
                var k = reader.ReadObject();
                var v = reader.ReadObject();

                values.Add(k, v);
            }

            reader.ReadEndMap();

            return new CborMap(values);
        }

        /// <summary>
        /// Attempts to read a <see cref="CborArray" /> from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="CborReader" /> to read from.</param>
        /// <param name="array">The array that was read, if any.</param>
        /// <returns><see langword="true" /> if an array was read; <see langword="false" /> otherwise.</returns>
        public static bool TryReadArray(this CborReader reader, out CborArray? array)
        {
            var state = reader.PeekState();

            if (state != CborReaderState.StartArray)
            {
                array = null;

                return false;
            }

            try
            {
                array = reader.ReadArray();

                return true;
            }
            catch (Exception e) when (e is InvalidOperationException || e is CborContentException)
            {
                array = null;

                return false;
            }
        }

        /// <summary>
        /// Attempts to read a <see cref="CborMap" /> from the reader.
        /// </summary>
        /// <param name="reader">The <see cref="CborReader" /> to read from.</param>
        /// <param name="map">The map that was read, if any.</param>
        /// <returns><see langword="true" /> if a map was read; <see langword="false" /> otherwise.</returns>
        public static bool TryReadMap(this CborReader reader, out CborMap? map)
        {
            var state = reader.PeekState();

            if (state != CborReaderState.StartMap)
            {
                map = null;

                return false;
            }

            try
            {
                map = reader.ReadMap();

                return true;
            }
            catch (Exception e) when (e is InvalidOperationException || e is CborContentException)
            {
                map = null;

                return false;
            }
        }
    }
}
