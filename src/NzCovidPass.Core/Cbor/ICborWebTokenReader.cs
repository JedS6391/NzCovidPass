namespace NzCovidPass.Core.Cbor
{
    public interface ICborWebTokenReader
    {
        bool TryReadToken(string base32Payload, out CborWebToken? token);
    }
}
