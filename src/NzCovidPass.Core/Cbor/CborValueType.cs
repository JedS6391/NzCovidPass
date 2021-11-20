namespace NzCovidPass.Core.Cbor
{
    /// <summary>
    /// Defines the available types of values encoded in the CBOR format.
    /// </summary>
    /// <remarks>
    /// Note that this is not an extensive list, as only what is needed for is supported
    /// </remarks>
    internal enum CborValueType
    {
        /// <summary>
        /// CBOR encoded map (major type five).
        /// </summary>
        Map,

        /// <summary>
        /// CBOR encoded array (major type four)
        /// </summary>
        Array,

        /// <summary>
        /// CBOR encoded integer (major types zero and one)
        /// </summary>
        Integer,

        /// <summary>
        /// CBOR encoded text (major type 3)
        /// </summary>
        TextString,

        /// <summary>
        /// CBOR encoded bytes (major type 2)
        /// </summary>
        ByteString
    }
}
