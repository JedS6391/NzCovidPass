using System;
using Microsoft.Extensions.Logging.Abstractions;
using NzCovidPass.Core.Cwt;
using Xunit;

namespace NzCovidPass.Test.Unit;

public class CwtSecurityTokenReaderTests
{
    private readonly CwtSecurityTokenReader _tokenReader;

    public CwtSecurityTokenReaderTests()
    {
        var logger = new NullLogger<CwtSecurityTokenReader>();

        _tokenReader = new CwtSecurityTokenReader(logger);
    }

    [Fact]
    public void ReadToken_InvalidBase32Payload_ReturnsFailResult()
    {
        const string Payload = "0KCEVIQEIVVWK6JNGEASNICZAEP2KALYDZSGSZB2O5SWEOTOPJRXALTDN53GSZBRHEXGQZLBNR2GQLTOPICRUYMBTIFAIGTUKBAAUYTWMOSGQQDDN5XHIZLYOSBHQJTIOR2HA4Z2F4XXO53XFZ3TGLTPOJTS6MRQGE4C6Y3SMVSGK3TUNFQWY4ZPOYYXQKTIOR2HA4Z2F4XW46TDOAXGG33WNFSDCOJONBSWC3DUNAXG46RPMNXW45DFPB2HGL3WGFTXMZLSONUW63TFGEXDALRQM12HS4DFQJ2FMZLSNFTGSYLCNRSUG4TFMRSW45DJMFWG6UDVMJWGSY2DN53GSZCQMFZXG4LDOJSWIZLOORUWC3CTOVRGUZLDOSRWSZ3JOZSW4TTBNVSWISTBMNVWUZTBNVUWY6KOMFWWKZ2TOBQXE4TPO5RWI33CNIYTSNRQFUYDILJRGYDVAYFE6VGU4MCDGK7DHLLYWHVPUS2YIDJOA6Y524TD3AZRM263WTY2BE4DPKIF27WKF3UDNNVSVWRDYIYVJ65IRJJJ6Z25M2DO4YZLBHWFQGVQR5ZLIWEQJOZTS3IQ7JTNCFDX";

        var context = new CwtSecurityTokenReaderContext(Payload);

        _tokenReader.ReadToken(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenReaderContext.InvalidBase32Payload, context.FailureReasons);
    }

    [Fact]
    public void ReadToken_InvalidMainCborObjectType_ReturnsFailResult()
    {
        // base32(cbor_encode({"test": "1"}))
        const string Payload = "UFSHIZLTORQTE===";

        var context = new CwtSecurityTokenReaderContext(Payload);

        _tokenReader.ReadToken(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure, context.FailureReasons);
    }

    [Fact]
    public void ReadToken_MissingCoseStructureComponent_ReturnsFailResult()
    {
        // base32(cbor_encode([h'A204456B65792D310126', h'1234']))
        const string Payload = "2KBEVIQEIVVWK6JNGEASMQQSGQ======";

        var context = new CwtSecurityTokenReaderContext(Payload);

        _tokenReader.ReadToken(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenReaderContext.InvalidCoseStructure, context.FailureReasons);
    }

    [Theory]
    [InlineData("2KCKAQQSGRBBENCCCI2A====")] // base32(cbor_encode(18([{}, h'1234', h'1234', h'1234']))) - first component not a byte string
    [InlineData("2KCEEERUIIJDIQQSGRBBENA=")] // base32(cbor_encode(18([h'1234', h'1234', h'1234', h'1234']))) - second component not an object
    [InlineData("2KCEEERUUCQEEERU")] // base32(cbor_encode(18([h'1234', {}, {}, h'1234']))) - third component component not a byte string
    [InlineData("2KCEEERUUBBBENFA")] // base32(cbor_encode(18([h'1234', {}, h'1234', {}]))) - fourth component component not a byte string
    public void ReadToken_InvalidCoseStructureComponentType_ReturnsFailResult(string payload)
    {
        var context = new CwtSecurityTokenReaderContext(payload);

        _tokenReader.ReadToken(context);

        AssertFailedResult(context);

        Assert.Contains(CwtSecurityTokenReaderContext.InvalidCoseStructure, context.FailureReasons);
    }

    [Fact]
    public void ReadToken_Valid_ReturnsSuccessResult()
    {
        const string Payload = "2KCEVIQEIVVWK6JNGEASNICZAEP2KALYDZSGSZB2O5SWEOTOPJRXALTDN53GSZBRHEXGQZLBNR2GQLTOPICRUYMBTIFAIGTUKBAAUYTWMOSGQQDDN5XHIZLYOSBHQJTIOR2HA4Z2F4XXO53XFZ3TGLTPOJTS6MRQGE4C6Y3SMVSGK3TUNFQWY4ZPOYYXQKTIOR2HA4Z2F4XW46TDOAXGG33WNFSDCOJONBSWC3DUNAXG46RPMNXW45DFPB2HGL3WGFTXMZLSONUW63TFGEXDALRQMR2HS4DFQJ2FMZLSNFTGSYLCNRSUG4TFMRSW45DJMFWG6UDVMJWGSY2DN53GSZCQMFZXG4LDOJSWIZLOORUWC3CTOVRGUZLDOSRWSZ3JOZSW4TTBNVSWISTBMNVWUZTBNVUWY6KOMFWWKZ2TOBQXE4TPO5RWI33CNIYTSNRQFUYDILJRGYDVAYFE6VGU4MCDGK7DHLLYWHVPUS2YIDJOA6Y524TD3AZRM263WTY2BE4DPKIF27WKF3UDNNVSVWRDYIYVJ65IRJJJ6Z25M2DO4YZLBHWFQGVQR5ZLIWEQJOZTS3IQ7JTNCFDX";

        var context = new CwtSecurityTokenReaderContext(Payload);

        _tokenReader.ReadToken(context);

        AssertSuccessResult(context);
    }

    private static void AssertFailedResult(CwtSecurityTokenReaderContext context)
    {
        Assert.NotNull(context);
        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
        Assert.NotEmpty(context.FailureReasons);
        Assert.Throws<InvalidOperationException>(() => context.Token);
    }

    private static void AssertSuccessResult(CwtSecurityTokenReaderContext context)
    {
        Assert.NotNull(context);
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.NotNull(context.Token);
        Assert.Empty(context.FailureReasons);
    }
}
