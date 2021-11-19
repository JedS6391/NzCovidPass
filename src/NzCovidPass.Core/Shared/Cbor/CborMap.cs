using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NzCovidPass.Core.Shared.Cbor
{
    internal sealed class CborMap : CborObject, IReadOnlyDictionary<CborObject, CborObject>
    {
        private readonly IDictionary<CborObject, CborObject> _values;

        public CborMap()
        {
            _values = new Dictionary<CborObject, CborObject>();
        }

        public CborMap(IDictionary<CborObject, CborObject> values)
        {
            _values = values;
        }

        public override CborValueType Type => CborValueType.Map;

        public CborObject this[CborObject key] => _values[key];

        public IEnumerable<CborObject> Keys => _values.Keys;

        public IEnumerable<CborObject> Values => _values.Values;

        public int Count => _values.Count;

        public bool ContainsKey(CborObject key) => _values.ContainsKey(key);

        public IEnumerator<KeyValuePair<CborObject, CborObject>> GetEnumerator() =>
            _values.GetEnumerator();

        public bool TryGetValue(CborObject key, [MaybeNullWhen(false)] out CborObject value) =>
            _values.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public IReadOnlyDictionary<object, object> ToGenericDictionary()
        {
            var dictionary = new Dictionary<object, object>(_values.Count);

            foreach (var item in _values)
            {
                var k = ConvertCborObject(item.Key);
                var v = ConvertCborObject(item.Value);

                dictionary.Add(k, v);
            }

            return dictionary;
        }

        private static object? ConvertCborObject(CborObject @object) => @object switch
        {
            CborMap map => map.ToGenericDictionary(),
            CborArray array => array.Select(v => ConvertCborObject(v)),
            CborByteString byteString => byteString.Value,
            CborTextString textString => textString.Value,
            CborInteger integer => integer.Value,
            CborBoolean boolean => boolean.Value,
            CborNull _ => null,
            _ => throw new NotSupportedException($"Unexpected CBOR object type '{@object.GetType().FullName}'.")
        };
    }
}
