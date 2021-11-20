namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Represents an array of CBOR encoded data items.
    /// </summary>
    internal sealed class CborArray : CborObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CborArray" /> class.
        /// </summary>
        /// <param name="values">The CBOR encoded data items contained in the array.</param>
        public CborArray(IEnumerable<CborObject> values)
        {
            Values = values.ToList();
        }

        /// <inheritdoc />
        public override CborValueType Type => CborValueType.Array;

        /// <summary>
        /// Gets the values of the array.
        /// </summary>
        public List<CborObject> Values { get; }

        /// <summary>
        /// Gets the number of values in the array.
        /// </summary>
        public int Count => Values.Count;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public CborObject this[int index] => Values[index];
    }
}
