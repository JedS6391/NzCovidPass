using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace NzCovidPass.Core.Cbor
{
    internal sealed class CborMap : CborObject
    {
        public CborMap(IDictionary<CborObject, CborObject> values)
        {
            Values = values;
        }

        public override CborValueType Type => CborValueType.Map;

        public IDictionary<CborObject, CborObject> Values { get; }

        public int Count => Values.Count;

        public IReadOnlyDictionary<object, object> ToGenericDictionary()
        {
            var dictionary = new Dictionary<object, object>(Values.Count);

            foreach (var item in Values)
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
            CborArray array => array.Values.Select(v => ConvertCborObject(v)),
            CborByteString byteString => byteString.Value,
            CborTextString textString => textString.Value,
            CborInteger integer => integer.Value,
            _ => throw new NotSupportedException($"Unexpected CBOR object type '{@object.GetType().FullName}'.")
        };
    }
}
