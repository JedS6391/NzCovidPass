namespace NzCovidPass.Core
{
    public class PassVerifierOptions
    {
        public string Prefix { get; set; }
        public int Version { get; set; }
        public IReadOnlySet<string> ValidIssuers { get; set; }

        public static class Defaults
        {
            public static readonly string Prefix = "NZCP:";
            public static readonly int Version = 1;
            public static readonly string[] TrustedIssuers = new string[]
            {
                "did:web:nzcp.identity.health.nz"
            };
        }
    }
}