using Dahomey.Cbor.ObjectModel;

namespace NzCovidPass.Core.Cbor
{
    public static class CborValueExtensions
    {        
        public static ReadOnlyMemory<byte> GetValueBytes(this CborValue cborValue) 
        {
            ArgumentNullException.ThrowIfNull(cborValue);

            var bytes = cborValue.Value<ReadOnlyMemory<byte>>();

            return bytes;
        }
    }
}