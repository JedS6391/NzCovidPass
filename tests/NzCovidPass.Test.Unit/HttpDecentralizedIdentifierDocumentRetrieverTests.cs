using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NzCovidPass.Core.Verification;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class HttpDecentralizedIdentifierDocumentRetrieverTests
{
    private readonly HttpDecentralizedIdentifierDocumentRetriever _documentRetriever;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MockHttpMessageHandler _httpMessageHandler;

    public HttpDecentralizedIdentifierDocumentRetrieverTests()
    {
        var logger = new NullLogger<HttpDecentralizedIdentifierDocumentRetriever>();

        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpMessageHandler = new MockHttpMessageHandler();

        _httpClientFactory
            .CreateClient(nameof(HttpDecentralizedIdentifierDocumentRetriever))
            .Returns(new HttpClient(_httpMessageHandler));

        _httpMessageHandler.RequestFunc = (request) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        _documentRetriever = new HttpDecentralizedIdentifierDocumentRetriever(logger, _httpClientFactory);
    }

    [Fact]
    public async Task GetDocumentAsync_SuccessfulResponseWithValidContent_ReturnsDocument()
    {
        const string Issuer = "nzcp.covid19.health.nz";

        _httpMessageHandler.RequestFunc = (request) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Documents.Valid)
            });

        var document = await _documentRetriever.GetDocumentAsync(Issuer);

        Assert.NotNull(document);
        Assert.NotEmpty(document.Contexts);
        Assert.Equal(new string[] { "https://w3.org/ns/did/v1" }, document.Contexts);
        Assert.Equal("did:web:nzcp.covid19.health.nz", document.Id);
        Assert.NotEmpty(document.VerificationMethods);
        Assert.Equal(1, document.VerificationMethods.Count);
        Assert.Equal("did:web:nzcp.covid19.health.nz#key-1", document.VerificationMethods[0].Id);
        Assert.Equal("did:web:nzcp.covid19.health.nz", document.VerificationMethods[0].Controller);
        Assert.Equal("JsonWebKey2020", document.VerificationMethods[0].Type);
        Assert.NotNull(document.VerificationMethods[0].PublicKey);
        Assert.Equal("EC", document.VerificationMethods[0].PublicKey.Kty);
        Assert.Equal("P-256", document.VerificationMethods[0].PublicKey.Crv);
        Assert.Equal("zRR-XGsCp12Vvbgui4DD6O6cqmhfPuXMhi1OxPl8760", document.VerificationMethods[0].PublicKey.X);
        Assert.Equal("Iv5SU6FuW-TRYh5_GOrJlcV_gpF_GpFQhCOD8LSk3T0", document.VerificationMethods[0].PublicKey.Y);
        Assert.NotEmpty(document.AssertionMethods);
        Assert.Equal(new string[] { "did:web:nzcp.covid19.health.nz#key-1" }, document.AssertionMethods);
    }

    [Fact]
    public async Task GetDocumentAsync_SuccessfulResponseWithInvalidContent_ThrowsJsonException()
    {
        const string Issuer = "nzcp.covid19.health.nz";

        _httpMessageHandler.RequestFunc = (request) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Documents.Invalid)
            });

        await Assert.ThrowsAsync<JsonException>(async () => await _documentRetriever.GetDocumentAsync(Issuer));
    }

    [Fact]
    public async Task GetDocumentAsync_NotSuccessfulResponseWithValidContent_ThrowsHttpRequestException()
    {
        const string Issuer = "nzcp.covid19.health.nz";

        _httpMessageHandler.RequestFunc = (request) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<HttpRequestException>(async() => await _documentRetriever.GetDocumentAsync(Issuer));
    }

    [Fact]
    public async Task GetDocumentAsync_HttpClientThrowsHttpRequestException_ThrowsHttpRequestException()
    {
        const string Issuer = "nzcp.covid19.health.nz";

        _httpMessageHandler.RequestFunc = (request) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException());

        await Assert.ThrowsAsync<HttpRequestException>(async() => await _documentRetriever.GetDocumentAsync(Issuer));
    }

    private static class Documents
    {
        public const string Valid = @"
{
  ""@context"": ""https://w3.org/ns/did/v1"",
  ""id"": ""did:web:nzcp.covid19.health.nz"",
  ""verificationMethod"": [
    {
      ""id"": ""did:web:nzcp.covid19.health.nz#key-1"",
      ""controller"": ""did:web:nzcp.covid19.health.nz"",
      ""type"": ""JsonWebKey2020"",
      ""publicKeyJwk"": {
        ""kty"": ""EC"",
        ""crv"": ""P-256"",
        ""x"": ""zRR-XGsCp12Vvbgui4DD6O6cqmhfPuXMhi1OxPl8760"",
        ""y"": ""Iv5SU6FuW-TRYh5_GOrJlcV_gpF_GpFQhCOD8LSk3T0""
      }
    }
  ],
  ""assertionMethod"": [
    ""did:web:nzcp.covid19.health.nz#key-1""
  ]
}";

        public const string Invalid = @"[ ""not valid"" ]";
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public MockHttpMessageHandler()
        {
        }

        public int RequestCount => SeenRequests.Count;

        public List<HttpRequestMessage> SeenRequests { get; } = new List<HttpRequestMessage>();

        public Func<HttpRequestMessage, Task<HttpResponseMessage>> RequestFunc { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SeenRequests.Add(request);

            var response = await RequestFunc.Invoke(request);

            response.RequestMessage = request;

            return response;
        }
    }
}
