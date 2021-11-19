using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NzCovidPass.Core;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Function
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddMemoryCache();

                    services.AddNzCovidPassVerifier(options =>
                    {
                        var validIssuers = PassVerifierOptions.Defaults.ValidIssuers.ToHashSet();

                        // Add test issuer
                        validIssuers.Add("did:web:nzcp.covid19.health.nz");

                        options.ValidIssuers = validIssuers;
                    });
                })
                .Build();

            host.Run();
        }
    }
}
