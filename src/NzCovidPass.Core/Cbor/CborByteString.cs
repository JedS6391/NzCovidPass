namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborByteString : CborObject
    {
        public CborByteString(byte[] value)
        {
            Value = value;
        }

        public override CborValueType Type => CborValueType.ByteString;
        public byte[] Value { get; }

        public override string ToString() => BitConverter.ToString(Value).Replace("-", string.Empty);
    }
}
