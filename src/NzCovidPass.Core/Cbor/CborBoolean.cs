namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborBoolean : CborObject
    {
        public CborBoolean(bool value)
        {
            Value = value;
        }

        public override CborValueType Type => CborValueType.Boolean;
        public bool Value { get; }

        public override string ToString() => Value.ToString();
    }
}
