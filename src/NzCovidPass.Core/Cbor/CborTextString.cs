namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Represents a CBOR encoded text value.
    /// </summary>
    internal sealed class CborTextString : CborObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CborTextString" /> class.
        /// </summary>
        /// <param name="value">The text value.</param>
        public CborTextString(string value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override CborValueType Type => CborValueType.TextString;

        /// <summary>
        /// Gets the text value.
        /// </summary>
        public string Value { get; }
    }
}
