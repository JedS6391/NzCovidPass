namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Represents a CBOR encoded sequence of bytes.
    /// </summary>
    internal sealed class CborByteString : CborObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CborByteString" /> class.
        /// </summary>
        /// <param name="value">The sequence of bytes.</param>
        public CborByteString(byte[] value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override CborValueType Type => CborValueType.ByteString;

        /// <summary>
        /// Gets the sequence of bytes.
        /// </summary>
        public byte[] Value { get; }
    }
}
