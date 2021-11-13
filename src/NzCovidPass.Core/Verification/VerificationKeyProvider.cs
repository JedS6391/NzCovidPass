using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Verification
{
    public class VerificationKeyProvider : IVerificationKeyProvider
    {
        private readonly ILogger<VerificationKeyProvider> _logger;
        private readonly IDecentralizedIdentifierDocumentRetriever _decentralizedIdentifierDocumentRetriever;

        public VerificationKeyProvider(
            ILogger<VerificationKeyProvider> logger,
            IDecentralizedIdentifierDocumentRetriever decentralizedIdentifierDocumentRetriever)
        {
            _logger = Requires.NotNull(logger);
            _decentralizedIdentifierDocumentRetriever = Requires.NotNull(decentralizedIdentifierDocumentRetriever);
        }

        public async Task<SecurityKey> GetKeyAsync(string issuer, string keyId)
        {
            _logger.LogDebug("Retrieving key with ID '{KeyId}' for issuer '{Issuer}'", keyId, issuer);

            // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys
            var keyReference = $"{issuer}#{keyId}";

            var decentralizedIdentifierDocument = await GetDecentralizedIdentifierDocumentAsync(issuer).ConfigureAwait(false);

            if (!decentralizedIdentifierDocument.AssertionMethods.Contains(keyReference))
            {
                _logger.LogError("Key reference '{KeyReference}' not found in assertion methods", keyReference);

                throw new KeyNotFoundException($"Unable to retrieve key '{keyReference}'.");
            }

            var verificationMethod = decentralizedIdentifierDocument
                .VerificationMethods
                .FirstOrDefault(vm => vm.Id == keyReference);

            if (verificationMethod is null || verificationMethod.Type != "JsonWebKey2020" || verificationMethod.PublicKey is null)
            {
                _logger.LogError("Key reference '{KeyReference}' not found in verification methods", keyReference);

                throw new KeyNotFoundException($"Unable to retrieve key '{keyReference}'.");
            }

            return verificationMethod.PublicKey;
        }

        private async Task<DecentralizedIdentifierDocument> GetDecentralizedIdentifierDocumentAsync(string issuer)
        {
            try
            {
                return await _decentralizedIdentifierDocumentRetriever
                    .GetDocumentAsync(issuer)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to retrieved decentralized identifier document");

                throw new KeyNotFoundException($"Unable to retrieve key.");
            }
        }
    }
}
