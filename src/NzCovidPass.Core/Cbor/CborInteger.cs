namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborInteger : CborObject
    {
        public CborInteger(int value)
        {
            Value = value;
        }

        public override CborValueType Type => CborValueType.Integer;
        public int Value { get; }

        public override string ToString() => Value.ToString();
    }
}
