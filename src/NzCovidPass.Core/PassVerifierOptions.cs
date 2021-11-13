using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core
{
    public class PassVerifierOptions
    {
        public string Prefix { get; set; }
        public int Version { get; set; }
        public IReadOnlySet<string> ValidIssuers { get; set; }
        public IReadOnlySet<string> ValidAlgorithms { get; set; }

        public static class Defaults
        {
            public static readonly string Prefix = "NZCP:";
            public static readonly int Version = 1;
            public static readonly string[] ValidIssuers = new string[]
            {
                "did:web:nzcp.identity.health.nz"
            };
            public static readonly string[] ValidAlgorithms = new string[]
            {
                SecurityAlgorithms.EcdsaSha256
            };
        }
    }
}