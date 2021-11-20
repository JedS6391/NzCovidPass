namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Represents a map of pairs of CBOR encoded data items.
    /// </summary>
    internal sealed class CborMap : CborObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CborMap" /> class.
        /// </summary>
        /// <param name="values">The pairs of CBOR encoded data items contained in the map.</param>
        public CborMap(IDictionary<CborObject, CborObject> values)
        {
            Values = values;
        }

        /// <inheritdoc />
        public override CborValueType Type => CborValueType.Map;

        /// <summary>
        /// Gets the values of the map.
        /// </summary>
        public IDictionary<CborObject, CborObject> Values { get; }

        /// <summary>
        /// Gets the number of pairs in the map.
        /// </summary>
        public int Count => Values.Count;

        /// <summary>
        /// Converts the map to a <see cref="IDictionary{TKey, TValue}" /> with all CBOR objects transformed
        /// to the raw value they represent.
        /// </summary>
        /// <returns>A generic dictionary that represents the CBOR map.</returns>
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

        private static object ConvertCborObject(CborObject @object) => @object switch
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
