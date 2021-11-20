using System.Collections;

namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborArray : CborObject, IEnumerable<CborObject>
    {
        private readonly List<CborObject> _values;

        public CborArray()
        {
            _values = new List<CborObject>();
        }

        public CborArray(IEnumerable<CborObject> values)
        {
            _values = values.ToList();
        }

        public override CborValueType Type => CborValueType.Array;

        public int Count => _values.Count;

        public CborObject this[int index] => _values[index];

        public IEnumerator<CborObject> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public override string ToString() => $"[{string.Join(", ", _values)}]";
    }
}
