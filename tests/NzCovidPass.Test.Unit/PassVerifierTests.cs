using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NzCovidPass.Core;
using NzCovidPass.Core.Cwt;
using NzCovidPass.Core.Models;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class PassVerifierTests
{
    private readonly PassVerifier _passVerifier;
    private readonly ICwtSecurityTokenReader _tokenReader;
    private readonly ICwtSecurityTokenValidator _tokenValidator;

    public PassVerifierTests()
    {
        var logger = new NullLogger<PassVerifier>();
        var optionsAccessor = Options.Create(new PassVerifierOptions());
        var tokenReader = Substitute.For<ICwtSecurityTokenReader>();
        var tokenValidator = Substitute.For<ICwtSecurityTokenValidator>();

        _passVerifier = new PassVerifier(logger, optionsAccessor, tokenReader, tokenValidator);
        _tokenReader = tokenReader;
        _tokenValidator = tokenValidator;
    }

    [Theory]
    [InlineData("NZCP:/1/.../..")]
    [InlineData("test/")]
    [InlineData(@"NZCP://1/...")]
    [InlineData("...")]
    public async Task VerifyAsync_InvalidNumberOfComponentsInPayload_ReturnsFailResult(string passPayload)
    {
        var result = await _passVerifier.VerifyAsync(passPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.InvalidPassComponents, result.FailureReasons);
    }

    [Theory]
    [InlineData("NZcp:/1/...")]
    [InlineData("invalid:/1/...")]
    public async Task VerifyAsync_InvalidPrefix_ReturnsFailResult(string passPayload)
    {
        var result = await _passVerifier.VerifyAsync(passPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.PrefixValidationFailed(validPrefix: "NZCP:"), result.FailureReasons);
    }

    [Theory]
    [InlineData("NZCP://...")]
    [InlineData("NZCP:/w/...")]
    [InlineData("NZCP:/2/...")]
    public async Task VerifyAsync_InvalidVersion_ReturnsFailResult(string passPayload)
    {
        var result = await _passVerifier.VerifyAsync(passPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.VersionValidationFailed(validVersion: 1), result.FailureReasons);
    }

    [Theory]
    [InlineData("NZCP:/1/")]
    [InlineData("NZCP:/1/  ")]
    public async Task VerifyAsync_EmptyPayload_ReturnsFailResult(string passPayload)
    {
        var result = await _passVerifier.VerifyAsync(passPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.EmptyPassPayload, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_TokenReadFailed_ReturnsFailResult()
    {
        const string PassPayload = "NZCP:/1/...";

        _tokenReader
            .When(r => r.ReadToken(Arg.Any<CwtSecurityTokenReaderContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CwtSecurityTokenReaderContext;

                context.Fail();
            });

        var result = await _passVerifier.VerifyAsync(PassPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.TokenReadFailed, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_TokenValidationFailed_ReturnsFailResult()
    {
        const string PassPayload = "NZCP:/1/...";

        _tokenReader
            .When(r => r.ReadToken(Arg.Any<CwtSecurityTokenReaderContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CwtSecurityTokenReaderContext;

                context.Succeed(CreateToken());
            });

        _tokenValidator
            .When(v => v.ValidateTokenAsync(Arg.Any<CwtSecurityTokenValidatorContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CwtSecurityTokenValidatorContext;

                context.Fail();
            });

        var result = await _passVerifier.VerifyAsync(PassPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.TokenValidationFailed, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_AllValidationsPassed_ReturnsSuccessResult()
    {
        const string PassPayload = "NZCP:/1/...";

        _tokenReader
            .When(r => r.ReadToken(Arg.Any<CwtSecurityTokenReaderContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CwtSecurityTokenReaderContext;

                context.Succeed(CreateToken());
            });

        _tokenValidator
            .When(v => v.ValidateTokenAsync(Arg.Any<CwtSecurityTokenValidatorContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CwtSecurityTokenValidatorContext;

                context.Succeed();
            });

        var result = await _passVerifier.VerifyAsync(PassPayload);

        AssertSuccessResult(result);
    }

    private static void AssertFailedResult(PassVerifierContext result)
    {
        Assert.NotNull(result);
        Assert.False(result.HasSucceeded);
        Assert.True(result.HasFailed);
        Assert.Throws<InvalidOperationException>(() => result.Token);
        Assert.Throws<InvalidOperationException>(() => result.Pass);
        Assert.NotEmpty(result.FailureReasons);
    }

    private static void AssertSuccessResult(PassVerifierContext result)
    {
        Assert.NotNull(result);
        Assert.True(result.HasSucceeded);
        Assert.False(result.HasFailed);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.Pass);
        Assert.Empty(result.FailureReasons);
    }

    private static CwtSecurityToken CreateToken() =>
        CwtSecurityTokenBuilder
            .New
            .WithKeyId("key-1")
            .WithAlgorithm(SecurityAlgorithms.EcdsaSha256)
            .WithCti(Guid.NewGuid())
            .WithIssuer("did:web:nzcp.identity.health.nz")
            .WithExpiry(DateTimeOffset.Now.AddMonths(6))
            .WithNotBefore(DateTimeOffset.Now.AddMonths(-1))
            .WithPublicCovidPassCredential(new VerifiableCredential<PublicCovidPass>(
                version: "1.0.0",
                context: new string[] { "https://www.w3.org/2018/credentials/v1", "https://nzcp.covid19.health.nz/contexts/v1" },
                type: new string[] { "VerifiableCredential", "PublicCovidPass" },
                credentialSubject: new PublicCovidPass(
                    givenName: "John Andrew",
                    familyName: "Doe",
                    dateOfBirth: new DateTimeOffset(new DateTime(1979, 4, 14))
                )))
            .Build();
}
