using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dahomey.Cbor.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NzCovidPass.Core;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Tokens;
using NzCovidPass.Core.Verification;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class CwtSecurityTokenValidatorTests
{
    private static readonly Random Random = new Random();

    private readonly CwtSecurityTokenValidator _tokenValidator;
    private readonly PassVerifierOptions _verifierOptions;
    private readonly IOptions<PassVerifierOptions> _verifierOptionsAccessor;
    private readonly IVerificationKeyProvider _verificationKeyProvider;

    public CwtSecurityTokenValidatorTests()
    {
        var logger = new NullLogger<CwtSecurityTokenValidator>();

        _verifierOptions = new PassVerifierOptions();
        _verifierOptionsAccessor = Substitute.For<IOptions<PassVerifierOptions>>();
        _verificationKeyProvider = Substitute.For<IVerificationKeyProvider>();

        _verifierOptionsAccessor
            .Value
            .Returns(_verifierOptions);

        _tokenValidator = new CwtSecurityTokenValidator(logger, _verifierOptionsAccessor, _verificationKeyProvider);
    }

    // Note that for some tests there can technically be multiple failure reasons, but for brevity
    // it is sufficient for each test to only validate the specific failure reason it expects is present.

    [Fact]
    public async Task ValidateTokenAsync_MissingKeyId_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder.New.Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.KeyIdValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_EmptyKeyId_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder.New.WithKeyId(string.Empty).Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.KeyIdValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_MissingAlgorithm_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder.New.WithKeyId("key-1").Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.AlgorithmValidationFailed(new string[] { SecurityAlgorithms.EcdsaSha256 }),
            context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidAlgorithm_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder.New.WithKeyId("key-1").WithAlgorithm(SecurityAlgorithms.Sha256).Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.AlgorithmValidationFailed(new string[] { SecurityAlgorithms.EcdsaSha256 }),
            context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_MissingTokenId_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder.New.WithKeyId("key-1").WithAlgorithm(SecurityAlgorithms.EcdsaSha256).Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.TokenIdValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_MissingIssuer_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.IssuerValidationFailed(new string[] { "did:web:nzcp.identity.health.nz" }),
            context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidIssuer_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("test-invalid-issuer")
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.IssuerValidationFailed(new string[] { "did:web:nzcp.identity.health.nz" }),
            context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_NotBeforeIsAfterExpiry_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddDays(1))
            .WithExpiry(DateTimeOffset.Now.AddDays(-1))
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.LifetimeValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_NotBeforeIsInTheFuture_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddDays(1))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.NotBeforeValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_ExpiryIsInThePast_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddDays(-1))
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.ExpiryValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_VerificationKeyProviderThrowsVerificationKeyNotFoundException_ReturnsFailResult()
    {
        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .Build();
        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromException<SecurityKey>(new VerificationKeyNotFoundException("Key not found")));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.VerificationKeyRetrievalFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_TokenAlgorithmDoesNotMatchKeyAlgorithm_ReturnsFailureResult()
    {
        SecurityKey? signingKey = null;

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.Sha512) // SHA-512
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                // Signature is computed using ECDSA w/ SHA-256
                var (key, signature) = ComputeSignature(header, payload);

                signingKey = key;

                return signature;
            })
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        // Override the valid algorithms since the token under test has a different algorithm than the default set
        _verifierOptions.ValidAlgorithms = new HashSet<string>(new string[]
        {
            SecurityAlgorithms.EcdsaSha256,
            SecurityAlgorithms.Sha512,
        });

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.SignatureValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_SignatureDoesNotMatch_ReturnsFailureResult()
    {
        SecurityKey? signingKey = null;

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                var (key, signature) = ComputeSignature(header, payload);

                // Modify the computed signature so it is no longer valid
                signature[Random.Next(signature.Length)] = (byte) Random.Next();

                signingKey = key;

                return signature;
            })
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.SignatureValidationFailed, context.FailureReasons);
    }

    [Fact]
    public async Task ValidateTokenAsync_MissingCredential_ReturnsFailureResult()
    {
        SecurityKey? signingKey = null;

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                var (key, signature) = ComputeSignature(header, payload);

                signingKey = key;

                return signature;
            })
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenValidatorContext.CredentialValidationFailed, context.FailureReasons);
    }

    [Theory]
    [InlineData()]
    [InlineData("https://www.w3.org/2018/credentials/v1")]
    [InlineData("https://nzcp.covid19.health.nz/contexts/v1")]
    [InlineData("https://www.w3.org/2018/credentials/v2", "https://nzcp.covid19.health.nz/contexts/v1")]
    public async Task ValidateTokenAsync_ExpectedContextMissing_ReturnsFailureResult(params string[] credentialContext)
    {
        SecurityKey? signingKey = null;

        var pass = new PublicCovidPass(
            givenName: "John Andrew",
            familyName: "Doe",
            dateOfBirth: new DateTimeOffset(new DateTime(1979, 4, 14)));

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                var (key, signature) = ComputeSignature(header, payload);

                signingKey = key;

                return signature;
            })
            .WithPublicCovidPassCredential(new VerifiableCredential<PublicCovidPass>(
                version: "1.0.0",
                context: credentialContext,
                type: new string[] { "VerifiableCredential", "PublicCovidPass" },
                credentialSubject: pass
            ))
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.CredentialContextValidationFailed(VerifiableCredential.BaseContext, pass.Context),
            context.FailureReasons);
    }

    [Theory]
    [InlineData()]
    [InlineData("VerifiableCredential")]
    [InlineData("PublicCovidPass")]
    [InlineData("VerifiableCredential", "PrivateCovidPass")]
    public async Task ValidateTokenAsync_ExpectedTypeMissing_ReturnsFailureResult(params string[] credentialType)
    {
        SecurityKey? signingKey = null;

        var pass = new PublicCovidPass(
            givenName: "John Andrew",
            familyName: "Doe",
            dateOfBirth: new DateTimeOffset(new DateTime(1979, 4, 14)));

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                var (key, signature) = ComputeSignature(header, payload);

                signingKey = key;

                return signature;
            })
            .WithPublicCovidPassCredential(new VerifiableCredential<PublicCovidPass>(
                version: "1.0.0",
                context: new string[] { "https://www.w3.org/2018/credentials/v1", "https://nzcp.covid19.health.nz/contexts/v1" },
                type: credentialType,
                credentialSubject: pass
            ))
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertFailedResult(context);

        Assert.Contains(
            CwtSecurityTokenValidatorContext.CredentialTypeValidationFailed(VerifiableCredential.BaseCredentialType, pass.Type),
            context.FailureReasons);
    }

   [Fact]
    public async Task ValidateTokenAsync_AllValidationsPass_ReturnsSuccessResult()
    {
        SecurityKey? signingKey = null;

        var token = CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-3))
            .WithExpiry(DateTimeOffset.Now.AddMonths(3))
            .WithSignatureFunc((header, payload) =>
            {
                var (key, signature) = ComputeSignature(header, payload);

                signingKey = key;

                return signature;
            })
            .WithPublicCovidPassCredential(new VerifiableCredential<PublicCovidPass>(
                version: "1.0.0",
                context: new string[] { "https://www.w3.org/2018/credentials/v1", "https://nzcp.covid19.health.nz/contexts/v1" },
                type: new string[] { "VerifiableCredential", "PublicCovidPass" },
                credentialSubject: new PublicCovidPass(
                givenName: "John Andrew",
                familyName: "Doe",
                dateOfBirth: new DateTimeOffset(new DateTime(1979, 4, 14)))
            ))
            .Build();

        var context = new CwtSecurityTokenValidatorContext(token);

        _verificationKeyProvider
            .GetKeyAsync(Arg.Is(token.Issuer), Arg.Is(token.KeyId))
            .Returns(Task.FromResult(signingKey));

        await _tokenValidator.ValidateTokenAsync(context);

        AssertSuccessResult(context);
    }

    private static void AssertFailedResult(CwtSecurityTokenValidatorContext context)
    {
        Assert.NotNull(context);
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        Assert.NotEmpty(context.FailureReasons);
        Assert.Null(context.Token.SigningKey);
    }

    private static void AssertSuccessResult(CwtSecurityTokenValidatorContext context)
    {
        Assert.NotNull(context);
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.NotNull(context.Token);
        Assert.Empty(context.FailureReasons);
    }

    private static (SecurityKey signingKey, byte[] signature) ComputeSignature(CwtSecurityToken.Header header, CwtSecurityToken.Payload payload)
    {
        // https://datatracker.ietf.org/doc/html/rfc8152#section-4.4
        // Note this process assumes a COSE_Sign1 structure, which NZ Covid passes should be.
        var b = new ArrayBufferWriter<byte>();
        var w = new CborWriter(b);

        w.WriteBeginArray(4);

        // context
        w.WriteString("Signature1");
        // body_protected
        w.WriteByteString(header.Bytes);
        // external_aad
        w.WriteByteString(Array.Empty<byte>());
        // payload
        w.WriteByteString(payload.Bytes);

        w.WriteEndArray(4);

        var signatureStructure = b.WrittenMemory.ToArray();

        var ecdSa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdSa);
        var signature = ecdSa.SignData(signatureStructure, HashAlgorithmName.SHA256);

        return (key, signature);
    }
}
