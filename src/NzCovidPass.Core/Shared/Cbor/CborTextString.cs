namespace NzCovidPass.Core.Shared.Cbor
{
    internal sealed class CborTextString : CborObject
    {
        public CborTextString(string value)
        {
            Value = value;
        }

        public override CborValueType Type => CborValueType.TextString;
        public string Value { get; }

        public override string ToString() => Value;
    }
}
