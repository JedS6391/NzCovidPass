using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Verification
{
    public interface IVerificationKeyProvider
    {
        Task<JsonWebKey> GetKeyAsync(string issuer, string keyId);
    }
}