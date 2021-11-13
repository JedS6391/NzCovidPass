using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core;
using NzCovidPass.Core.Shared;

const string CovidPassValid = "NZCP:/1/2KCEVIQEIVVWK6JNGEASNICZAEP2KALYDZSGSZB2O5SWEOTOPJRXALTDN53GSZBRHEXGQZLBNR2GQLTOPICRUYMBTIFAIGTUKBAAUYTWMOSGQQDDN5XHIZLYOSBHQJTIOR2HA4Z2F4XXO53XFZ3TGLTPOJTS6MRQGE4C6Y3SMVSGK3TUNFQWY4ZPOYYXQKTIOR2HA4Z2F4XW46TDOAXGG33WNFSDCOJONBSWC3DUNAXG46RPMNXW45DFPB2HGL3WGFTXMZLSONUW63TFGEXDALRQMR2HS4DFQJ2FMZLSNFTGSYLCNRSUG4TFMRSW45DJMFWG6UDVMJWGSY2DN53GSZCQMFZXG4LDOJSWIZLOORUWC3CTOVRGUZLDOSRWSZ3JOZSW4TTBNVSWISTBMNVWUZTBNVUWY6KOMFWWKZ2TOBQXE4TPO5RWI33CNIYTSNRQFUYDILJRGYDVAYFE6VGU4MCDGK7DHLLYWHVPUS2YIDJOA6Y524TD3AZRM263WTY2BE4DPKIF27WKF3UDNNVSVWRDYIYVJ65IRJJJ6Z25M2DO4YZLBHWFQGVQR5ZLIWEQJOZTS3IQ7JTNCFDX";

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Debug))
    .ConfigureServices((_, services) =>
    {
        services.AddNzCovidPassVerifier(
            options =>
            {
                var validIssuers = PassVerifierOptions.Defaults.ValidIssuers.ToHashSet();

                // Add test issuer
                validIssuers.Add("did:web:nzcp.covid19.health.nz");

                options.Prefix = PassVerifierOptions.Defaults.Prefix;
                options.Version = PassVerifierOptions.Defaults.Version;
                options.ValidIssuers = validIssuers;
                options.ValidAlgorithms = PassVerifierOptions.Defaults.ValidAlgorithms.ToHashSet();
            }
        );
    })
    .Build();

var verifier = host.Services.GetRequiredService<PassVerifier>();

var result = await verifier.VerifyAsync(CovidPassValid);

if (result.HasSucceeded)
{
    var details = result.Credentials.Details;

    Console.WriteLine($"NZ COVID Pass subject details: {details.FamilyName}, {details.GivenName} - {details.DateOfBirth}");
}
else
{
    Console.WriteLine($"Verification failed: {string.Join(", ", result.FailureReasons.Select(fr => fr.Code))}");
}
