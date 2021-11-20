using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NzCovidPass.Core;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Verification;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class DecentralizedIdentifierDocumentVerificationKeyProviderTests
{
    private readonly DecentralizedIdentifierDocumentVerificationKeyProvider _verificationKeyProvider;
    private readonly PassVerifierOptions _verifierOptions;
    private readonly IOptions<PassVerifierOptions> _verifierOptionsAccessor;
    private readonly IDecentralizedIdentifierDocumentRetriever _decentralizedIdentifierDocumentRetriever;
    private readonly IMemoryCache _securityKeyCache;

    public DecentralizedIdentifierDocumentVerificationKeyProviderTests()
    {
        var logger = new NullLogger<DecentralizedIdentifierDocumentVerificationKeyProvider>();

        _verifierOptions = new PassVerifierOptions();
        _verifierOptionsAccessor = Substitute.For<IOptions<PassVerifierOptions>>();
        _decentralizedIdentifierDocumentRetriever = Substitute.For<IDecentralizedIdentifierDocumentRetriever>();
        _securityKeyCache = Substitute.For<IMemoryCache>();

        _verifierOptionsAccessor
            .Value
            .Returns(_verifierOptions);

        _securityKeyCache
            .TryGetValue(Arg.Any<object>(), out Arg.Any<object>())
            .Returns(false);

        _verificationKeyProvider = new DecentralizedIdentifierDocumentVerificationKeyProvider(
            logger,
            _verifierOptionsAccessor,
            _decentralizedIdentifierDocumentRetriever,
            _securityKeyCache);
    }

    [Fact]
    public async Task GetKeyAsync_KeyFoundInCache_ReturnsKeyFromCache()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        var expectedKey = new TestSecurityKey();

        _securityKeyCache
            .TryGetValue(Arg.Is($"{Issuer}#{KeyId}"), out Arg.Any<SecurityKey>())
            .Returns(ci =>
            {
                ci[1] = expectedKey;

                return true;
            });

        var key = await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId);

        Assert.NotNull(key);
        Assert.Equal(expectedKey, key);
    }

    [Fact]
    public async Task GetKeyAsync_DecentralizedIdentifierDocumentRetrieverThrows_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromException<DecentralizedIdentifierDocument>(new Exception()));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_DecentralizedIdentifierDocumentRetrieverReturnsNull_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(null));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_DidDocumentAssertionMethodsDoesNotContainKeyReference_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(new DecentralizedIdentifierDocument(
                id: Issuer,
                contexts: new string[] { "https://w3.org/ns/did/v1" },
                verificationMethods: Array.Empty<DecentralizedIdentifierDocument.VerificationMethod>(),
                assertionMethods: Array.Empty<string>()
            )));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_DidDocumentVerificationMethodsDoesNotContainKeyReference_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(new DecentralizedIdentifierDocument(
                id: Issuer,
                contexts: new string[] { "https://w3.org/ns/did/v1" },
                verificationMethods: Array.Empty<DecentralizedIdentifierDocument.VerificationMethod>(),
                assertionMethods: new string[] { $"{Issuer}#{KeyId}" }
            )));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_DidDocumentVerificationMethodIncorrectKeyType_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(new DecentralizedIdentifierDocument(
                id: Issuer,
                contexts: new string[] { "https://w3.org/ns/did/v1" },
                verificationMethods: new DecentralizedIdentifierDocument.VerificationMethod[]
                {
                    new DecentralizedIdentifierDocument.VerificationMethod(
                        id: $"{Issuer}#{KeyId}",
                        controller: Issuer,
                        type: "InvalidKeyType",
                        publicKey: new JsonWebKey()
                        {
                            Kty = "EC",
                            Crv = "P-256",
                            X = "zRR-XGsCp12Vvbgui4DD6O6cqmhfPuXMhi1OxPl8760",
                            Y = "Iv5SU6FuW-TRYh5_GOrJlcV_gpF_GpFQhCOD8LSk3T0"
                        })
                },
                assertionMethods: new string[] { $"{Issuer}#{KeyId}" }
            )));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_DidDocumentVerificationMethodPublicKeyNull_ThrowsVerificationKeyNotFoundException()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(new DecentralizedIdentifierDocument(
                id: Issuer,
                contexts: new string[] { "https://w3.org/ns/did/v1" },
                verificationMethods: new DecentralizedIdentifierDocument.VerificationMethod[]
                {
                    new DecentralizedIdentifierDocument.VerificationMethod(
                        id: $"{Issuer}#{KeyId}",
                        controller: Issuer,
                        type: "JsonWebKey2020",
                        publicKey: null)
                },
                assertionMethods: new string[] { $"{Issuer}#{KeyId}" }
            )));

        await Assert.ThrowsAsync<VerificationKeyNotFoundException>(async () => await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId));
    }

    [Fact]
    public async Task GetKeyAsync_KeyFound_ReturnsKey()
    {
        const string Issuer = "test-issuer";
        const string KeyId = "test-key";

        var expectedKey = new JsonWebKey()
        {
            Kty = "EC",
            Crv = "P-256",
            X = "zRR-XGsCp12Vvbgui4DD6O6cqmhfPuXMhi1OxPl8760",
            Y = "Iv5SU6FuW-TRYh5_GOrJlcV_gpF_GpFQhCOD8LSk3T0"
        };

        _decentralizedIdentifierDocumentRetriever
            .GetDocumentAsync(Arg.Is(Issuer))
            .Returns(Task.FromResult<DecentralizedIdentifierDocument>(new DecentralizedIdentifierDocument(
                id: Issuer,
                contexts: new string[] { "https://w3.org/ns/did/v1" },
                verificationMethods: new DecentralizedIdentifierDocument.VerificationMethod[]
                {
                    new DecentralizedIdentifierDocument.VerificationMethod(
                        id: $"{Issuer}#{KeyId}",
                        controller: Issuer,
                        type: "JsonWebKey2020",
                        publicKey: expectedKey)
                },
                assertionMethods: new string[] { $"{Issuer}#{KeyId}" }
            )));

        var key = await _verificationKeyProvider.GetKeyAsync(Issuer, KeyId);

        Assert.NotNull(key);
        Assert.Equal(expectedKey, key);
    }


    private class TestSecurityKey : SecurityKey
    {
        public override int KeySize => 256;
    }
}
