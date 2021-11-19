namespace NzCovidPass.Core.Shared.Cbor
{
    internal abstract class CborObject
    {
        public abstract CborValueType Type { get; }
    }
}
