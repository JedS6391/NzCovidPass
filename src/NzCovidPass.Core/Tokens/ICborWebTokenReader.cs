namespace NzCovidPass.Core.Tokens
{
    public interface ICborWebTokenReader
    {
        bool TryReadToken(string base32Payload, out CborWebToken? token);
    }
}
