namespace NzCovidPass.Core.Shared.Cbor
{
    internal sealed class CborNull : CborObject
    {
        public static readonly CborNull Value = new CborNull();

        private CborNull()
        {
        }

        public override CborValueType Type => CborValueType.Null;
    }
}
