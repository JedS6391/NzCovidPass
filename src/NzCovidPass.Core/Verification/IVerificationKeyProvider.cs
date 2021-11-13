using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Verification
{
    public interface IVerificationKeyProvider
    {
        Task<SecurityKey> GetKeyAsync(string issuer, string keyId);
    }
}