using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Verification
{
    public class VerificationKeyProvider : IVerificationKeyProvider
    {
        private readonly IDecentralizedIdentifierDocumentRetriever _decentralizedIdentifierDocumentRetriever;

        public VerificationKeyProvider(IDecentralizedIdentifierDocumentRetriever decentralizedIdentifierDocumentRetriever)
        {
            _decentralizedIdentifierDocumentRetriever = Requires.NotNull(decentralizedIdentifierDocumentRetriever);
        }

        public JsonWebKey GetKeyAsync(string issuer, string keyId)
        {
            return null;
        }
    }
}