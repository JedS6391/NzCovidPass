using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly PassVerifierOptions _verifierOptions;
        private readonly IDecentralizedIdentifierDocumentRetriever _decentralizedIdentifierDocumentRetriever;
        private readonly IMemoryCache _securityKeyCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecentralizedIdentifierDocumentVerificationKeyProvider" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        /// <param name="verifierOptionsAccessor">An accessor for <see cref="PassVerifierOptions" /> instances.</param>
        /// <param name="decentralizedIdentifierDocumentRetriever">An <see cref="IDecentralizedIdentifierDocumentRetriever" /> instance used to obtain DID documents.</param>
        /// <param name="securityKeyCache">An <see cref="IMemoryCache" /> used for temporarily store resolved keys.</param>
        public DecentralizedIdentifierDocumentVerificationKeyProvider(
            ILogger<DecentralizedIdentifierDocumentVerificationKeyProvider> logger,
            IOptions<PassVerifierOptions> verifierOptionsAccessor,
            IDecentralizedIdentifierDocumentRetriever decentralizedIdentifierDocumentRetriever,
            IMemoryCache securityKeyCache)
        {
            _logger = Requires.NotNull(logger);
            _verifierOptions = Requires.NotNull(verifierOptionsAccessor).Value;
            _decentralizedIdentifierDocumentRetriever = Requires.NotNull(decentralizedIdentifierDocumentRetriever);
            _securityKeyCache = Requires.NotNull(securityKeyCache);
        }

        /// <inheritdoc />
        public async Task<SecurityKey> GetKeyAsync(string issuer, string keyId)
        {
            var keyReference = GenerateKeyReference(issuer, keyId);

            if (_securityKeyCache.TryGetValue<SecurityKey>(keyReference, out var securityKey))
            {
                _logger.LogDebug("Obtained key with ID '{KeyId}' for issuer '{Issuer}' from cache.", keyId, issuer);

                return securityKey;
            }

            _logger.LogDebug("Retrieving key with ID '{KeyId}' for issuer '{Issuer}'", keyId, issuer);

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

            var key = verificationMethod.PublicKey;

            _securityKeyCache.Set(keyReference, key, absoluteExpirationRelativeToNow: _verifierOptions.SecurityKeyCacheTime);

            return key;
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

                throw new VerificationKeyNotFoundException($"Unable to retrieve key for issuer '{issuer}'.");
            }
        }

        // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys
        private static string GenerateKeyReference(string issuer, string keyId) => $"{issuer}#{keyId}";
    }
}
