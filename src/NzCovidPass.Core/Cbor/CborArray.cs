namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborArray : CborObject
    {
        public CborArray(IEnumerable<CborObject> values)
        {
            Values = values.ToList();
        }

        public override CborValueType Type => CborValueType.Array;

        public List<CborObject> Values { get; }

        public int Count => Values.Count;

        public CborObject this[int index] => Values[index];
    }
}
