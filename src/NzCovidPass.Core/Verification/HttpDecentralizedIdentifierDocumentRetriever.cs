using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Verification
{
    /// <summary>
    /// An <see cref="IDecentralizedIdentifierDocumentRetriever" /> implementation that retrieves documents via HTTP.
    /// </summary>
    public class HttpDecentralizedIdentifierDocumentRetriever : IDecentralizedIdentifierDocumentRetriever
    {
        private const string DidDocumentPath = ".well-known/did.json";

        private readonly ILogger<HttpDecentralizedIdentifierDocumentRetriever> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDecentralizedIdentifierDocumentRetriever" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        /// <param name="httpClientFactory">An <see cref="IHttpClientFactory" /> instance used to create clients for HTTP communication.</param>
        public HttpDecentralizedIdentifierDocumentRetriever(
            ILogger<HttpDecentralizedIdentifierDocumentRetriever> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = Requires.NotNull(logger);
            _httpClientFactory = Requires.NotNull(httpClientFactory);
        }

        /// <inheritdoc />
        public async Task<DecentralizedIdentifierDocument> GetDocumentAsync(string issuer)
        {
            // See https://nzcp.covid19.health.nz/#example-resolving-an-issuers-identifier-to-their-public-keys
            var host = issuer.Replace("did:web:", string.Empty);
            var uriBuilder = new UriBuilder(Uri.UriSchemeHttps, host)
            {
                Path = DidDocumentPath
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
