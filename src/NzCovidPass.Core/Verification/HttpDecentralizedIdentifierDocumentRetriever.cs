using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Verification
{
    public class HttpDecentralizedIdentifierDocumentRetriever : IDecentralizedIdentifierDocumentRetriever
    {
        private readonly ILogger<HttpDecentralizedIdentifierDocumentRetriever> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpDecentralizedIdentifierDocumentRetriever(
            ILogger<HttpDecentralizedIdentifierDocumentRetriever> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = Requires.NotNull(logger);
            _httpClientFactory = Requires.NotNull(httpClientFactory);
        }

        public async Task<DecentralizedIdentifierDocument> GetDocumentAsync(string issuer)
        {
            // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys
            var host = issuer.Replace("did:web:", string.Empty);
            var uriBuilder = new UriBuilder(Uri.UriSchemeHttps, host)
            {
                Path = ".well-known/did.json"
            };

            var client = _httpClientFactory.CreateClient(nameof(HttpDecentralizedIdentifierDocumentRetriever));

            _logger.LogDebug("Retrieving DID document at address '{Address}'", uriBuilder.Uri);

            var response = await client
                .GetAsync(uriBuilder.Uri)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var document = await response
                .Content
                .ReadFromJsonAsync<DecentralizedIdentifierDocument>()
                .ConfigureAwait(false);

            _logger.LogDebug("Successfully retrieved DID document '{Document}'", document);

            return document;
        }
    }
}
