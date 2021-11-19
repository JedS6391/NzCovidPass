using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NzCovidPass.Console;
using NzCovidPass.Core;
using NzCovidPass.Core.Shared;

await Parser
    .Default
    .ParseArguments<Options>(args)
    .WithNotParsed(errors => Console.WriteLine($"Failed to parse options."))
    .WithParsedAsync(async options =>
    {
        var host = BuildHost(args, options.Verbose);

        var verifier = host.Services.GetRequiredService<PassVerifier>();

        var result = await verifier.VerifyAsync(options.Pass);

        if (result.HasSucceeded)
        {
            Console.WriteLine($"NZ COVID Pass subject details: {result.Pass.FamilyName}, {result.Pass.GivenName} - {result.Pass.DateOfBirth}");
        }
        else
        {
            Console.WriteLine($"Verification failed: {string.Join(", ", result.FailureReasons.Select(fr => fr.Code))}");
        }
    });

static IHost BuildHost(string[] args, bool verbose) => Host
    .CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.SetMinimumLevel(verbose ? LogLevel.Trace : LogLevel.Information))
    .ConfigureServices((_, services) =>
    {
        services.AddMemoryCache();

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

