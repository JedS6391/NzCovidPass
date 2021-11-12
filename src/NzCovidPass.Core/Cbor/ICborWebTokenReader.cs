namespace NzCovidPass.Core.Cbor
{
    public interface ICborWebTokenReader
    {
        bool TryReadToken(byte[] data, out CborWebToken? token);
    }
}