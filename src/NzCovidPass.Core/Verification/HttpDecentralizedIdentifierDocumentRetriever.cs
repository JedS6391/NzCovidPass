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

        public async Task<DecentralizedIdentifierDocument> GetDocumentAsync()
        {            
            var client = _httpClientFactory.CreateClient(nameof(HttpDecentralizedIdentifierDocumentRetriever));

            _logger.LogDebug("Retrieving DID document at base address '{BaseAddress}'", client.BaseAddress);

            var response = await client.GetAsync(".well-known/did.json").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "{Method} request to '{Uri}' failed with status code '{StatusCode}'",
                    response.RequestMessage?.Method,
                    response.RequestMessage?.RequestUri,
                    response.StatusCode);

                // TODO
                throw new Exception();
            }

            var document = await response
                .Content
                .ReadFromJsonAsync<DecentralizedIdentifierDocument>()
                .ConfigureAwait(false);

            _logger.LogDebug("Successfully retrieved DID document '{Document}'", document);

            return document;
        }
    }
}