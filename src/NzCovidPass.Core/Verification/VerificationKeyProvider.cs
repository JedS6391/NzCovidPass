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

        public async Task<JsonWebKey> GetKeyAsync(string issuer, string keyId)
        {
            var decentralizedIdentifierDocument = await _decentralizedIdentifierDocumentRetriever
                .GetDocumentAsync(issuer)
                .ConfigureAwait(false);

            // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys            
            var keyReference = $"{issuer}#{keyId}";

            if (!decentralizedIdentifierDocument.AssertionMethods.Contains(keyReference))
            {
                // TODO
                throw new Exception();
            }

            var verificationMethod = decentralizedIdentifierDocument
                .VerificationMethods
                .FirstOrDefault(vm => vm.Id == keyReference);

            if (verificationMethod is null || verificationMethod.Type != "JsonWebKey2020" || verificationMethod.PublicKey is null)
            {
                // TODO
                throw new Exception();
            }

            return verificationMethod.PublicKey;
        }
    }
}