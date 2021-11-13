namespace NzCovidPass.Core.Tokens
{
    public interface ICborWebTokenValidator
    {
        Task ValidateTokenAsync(CborWebTokenValidatorContext context);
    }
}
