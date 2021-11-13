using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Verification
{
    /// <summary>
    /// An <see cref="IVerificationKeyProvider" /> implementation that resolves keys from a Decentralized Identifier (DID) document.
    /// </summary>
    public class DecentralizedIdentifierDocumentVerificationKeyProvider : IVerificationKeyProvider
    {
        private const string ValidVerificationMethodType = "JsonWebKey2020";

        private readonly ILogger<DecentralizedIdentifierDocumentVerificationKeyProvider> _logger;
        private readonly IDecentralizedIdentifierDocumentRetriever _decentralizedIdentifierDocumentRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecentralizedIdentifierDocumentVerificationKeyProvider" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        /// <param name="decentralizedIdentifierDocumentRetriever">An <see cref="IDecentralizedIdentifierDocumentRetriever" /> instance used to obtain DID documents.</param>
        public DecentralizedIdentifierDocumentVerificationKeyProvider(
            ILogger<DecentralizedIdentifierDocumentVerificationKeyProvider> logger,
            IDecentralizedIdentifierDocumentRetriever decentralizedIdentifierDocumentRetriever)
        {
            _logger = Requires.NotNull(logger);
            _decentralizedIdentifierDocumentRetriever = Requires.NotNull(decentralizedIdentifierDocumentRetriever);
        }

        /// <inheritdoc />
        public async Task<SecurityKey> GetKeyAsync(string issuer, string keyId)
        {
            _logger.LogDebug("Retrieving key with ID '{KeyId}' for issuer '{Issuer}'", keyId, issuer);

            // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys
            var keyReference = $"{issuer}#{keyId}";

            var decentralizedIdentifierDocument = await GetDecentralizedIdentifierDocumentAsync(issuer).ConfigureAwait(false);

            if (!decentralizedIdentifierDocument.AssertionMethods.Contains(keyReference))
            {
                _logger.LogError("Key reference '{KeyReference}' not found in assertion methods", keyReference);

                throw new VerificationKeyNotFoundException($"Unable to retrieve key '{keyReference}'.");
            }

            var verificationMethod = decentralizedIdentifierDocument
                .VerificationMethods
                .FirstOrDefault(vm => vm.Id == keyReference);

            if (verificationMethod is null || verificationMethod.Type != ValidVerificationMethodType || verificationMethod.PublicKey is null)
            {
                _logger.LogError("Key reference '{KeyReference}' not found in verification methods", keyReference);

                throw new VerificationKeyNotFoundException($"Unable to retrieve key '{keyReference}'.");
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

                throw new VerificationKeyNotFoundException($"Unable to retrieve key.");
            }
        }
    }
}
