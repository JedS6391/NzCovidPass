using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Verification
{
    /// <summary>
    /// Provides <see cref="SecurityKey" /> instances used to perform the cryptographic verification of digital signatures.
    /// </summary>
    public interface IVerificationKeyProvider
    {
        /// <summary>
        /// Gets the <see cref="SecurityKey" /> associated with the provided details.
        /// </summary>
        /// <param name="issuer">The issuer associated with the key to obtain.</param>
        /// <param name="keyId">The identifier of the key to obtain.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the resolved <see cref="SecurityKey" />.</returns>
        Task<SecurityKey> GetKeyAsync(string issuer, string keyId);
    }
}
