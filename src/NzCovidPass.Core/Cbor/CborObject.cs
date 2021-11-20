namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Defines the base properties of all CBOR encoded objects.
    /// </summary>
    internal abstract class CborObject
    {
        /// <summary>
        /// Gets the type of value the CBOR encoded object represents.
        /// </summary>
        public abstract CborValueType Type { get; }
    }
}
