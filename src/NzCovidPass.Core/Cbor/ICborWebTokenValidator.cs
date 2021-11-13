namespace NzCovidPass.Core.Cbor
{
    public interface ICborWebTokenValidator
    {
        Task<CborWebTokenValidatorContext> ValidateTokenAsync(CborWebToken token);
    }
}