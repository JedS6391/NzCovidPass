namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Represents a CBOR encoded integer.
    /// </summary>
    internal sealed class CborInteger : CborObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CborInteger" /> class.
        /// </summary>
        /// <param name="value">The integer value.</param>
        public CborInteger(int value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override CborValueType Type => CborValueType.Integer;

        /// <summary>
        /// Gets the integer value.
        /// </summary>
        public int Value { get; }
    }
}
