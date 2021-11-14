using System;
using System.Linq;
using System.Threading.Tasks;
using Dahomey.Cbor.ObjectModel;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NzCovidPass.Core;
using NzCovidPass.Core.Tokens;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class PassVerifierTests
{
    private readonly PassVerifier _passVerifier;
    private readonly ICborWebTokenReader _tokenReader;
    private readonly ICborWebTokenValidator _tokenValidator;

    public PassVerifierTests()
    {
        var logger = new NullLogger<PassVerifier>();
        var optionsAccessor = Options.Create(new PassVerifierOptions()
        {
            Prefix = PassVerifierOptions.Defaults.Prefix,
            Version = PassVerifierOptions.Defaults.Version,
            ValidIssuers = PassVerifierOptions.Defaults.ValidIssuers.ToHashSet(),
            ValidAlgorithms = PassVerifierOptions.Defaults.ValidAlgorithms.ToHashSet()
        });
        var tokenReader = Substitute.For<ICborWebTokenReader>();
        var tokenValidator = Substitute.For<ICborWebTokenValidator>();

        _passVerifier = new PassVerifier(logger, optionsAccessor, tokenReader, tokenValidator);
        _tokenReader = tokenReader;
        _tokenValidator = tokenValidator;
    }

    [Theory]
    [InlineData("NZCP:/1/.../..")]
    [InlineData("test/")]
    [InlineData(@"NZCP://1/...")]
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

        Assert.Contains(PassVerifierContext.PrefixValidationFailed, result.FailureReasons);
    }

    [Theory]
    [InlineData("NZCP://...")]
    [InlineData("NZCP:/w/...")]
    [InlineData("NZCP:/2/...")]
    public async Task VerifyAsync_InvalidVersion_ReturnsFailResult(string passPayload)
    {
        var result = await _passVerifier.VerifyAsync(passPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.VersionValidationFailed, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_EmptyPayload_ReturnsFailResult()
    {
        const string PassPayload = "NZCP:/1/";

        var result = await _passVerifier.VerifyAsync(PassPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.InvalidPassPayload, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_TokenReadFailed_ReturnsFailResult()
    {
        const string PassPayload = "NZCP:/1/...";

        _tokenReader
            .TryReadToken(Arg.Any<string>(), out Arg.Any<CborWebToken>())
            .Returns(false);

        var result = await _passVerifier.VerifyAsync(PassPayload);

        AssertFailedResult(result);

        Assert.Contains(PassVerifierContext.TokenReadFailed, result.FailureReasons);
    }

    [Fact]
    public async Task VerifyAsync_TokenReaderReturnsNoToken_ReturnsFailResult()
    {
        const string PassPayload = "NZCP:/1/...";

        _tokenReader
            .TryReadToken(Arg.Any<string>(), out Arg.Any<CborWebToken>())
            .Returns(ci =>
            {
                // Set token to null but return successful token read
                ci[1] = null;

                return true;
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
            .TryReadToken(Arg.Any<string>(), out Arg.Any<CborWebToken>())
            .Returns(ci =>
            {
                // Set token
                ci[1] = CreateToken();

                return true;
            });

        _tokenValidator
            .When(v => v.ValidateTokenAsync(Arg.Any<CborWebTokenValidatorContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CborWebTokenValidatorContext;

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
            .TryReadToken(Arg.Any<string>(), out Arg.Any<CborWebToken>())
            .Returns(ci =>
            {
                // Set token
                ci[1] = CreateToken();

                return true;
            });

        _tokenValidator
            .When(v => v.ValidateTokenAsync(Arg.Any<CborWebTokenValidatorContext>()))
            .Do(ci =>
            {
                var context = ci[0] as CborWebTokenValidatorContext;

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
        Assert.Throws<InvalidOperationException>(() => result.Credentials);
        Assert.NotEmpty(result.FailureReasons);
    }

    private static void AssertSuccessResult(PassVerifierContext result)
    {
        Assert.NotNull(result);
        Assert.True(result.HasSucceeded);
        Assert.False(result.HasFailed);
        Assert.NotNull(result.Token);
        Assert.Empty(result.FailureReasons);
    }

    private static CborWebToken CreateToken()
    {
        return new CborWebToken(
            new CborWebToken.Header(new CborObject(), ReadOnlyMemory<byte>.Empty),
            new CborWebToken.Payload(new CborObject(), ReadOnlyMemory<byte>.Empty),
            new CborWebToken.Signature(ReadOnlyMemory<byte>.Empty));
    }
}