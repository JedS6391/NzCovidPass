using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Verification
{
    public interface IVerificationKeyProvider
    {
        JsonWebKey GetKeyAsync(string issuer, string keyId);
    }
}