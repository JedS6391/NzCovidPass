using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core
{
    /// <summary>
    /// Provides the options available to <see cref="PassVerifier" />.
    /// </summary>
    public class PassVerifierOptions
    {
        /// <summary>
        /// Gets or sets the value that is required in the payload prefix component.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value that is required in the payload version component.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the collection of valid issuers that will be used to check against the token's issuer.
        /// </summary>
        public IReadOnlySet<string> ValidIssuers { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets or sets the collection of valid algorithms that will be used to check against the token's algorithm.
        /// </summary>        
        public IReadOnlySet<string> ValidAlgorithms { get; set; } = new HashSet<string>();

        /// <summary>
        /// Defines default values for <see cref="PassVerifierOptions" />.
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// The default prefix.
            /// </summary>
            public static readonly string Prefix = "NZCP:";

            /// <summary>
            /// The default version.
            /// </summary>
            public static readonly int Version = 1;

            /// <summary>
            /// The default valid issuers.
            /// </summary>
            public static readonly string[] ValidIssuers = new string[]
            {
                "did:web:nzcp.identity.health.nz"
            };

            /// <summary>
            /// The default valid algorithms.
            /// </summary>            
            public static readonly string[] ValidAlgorithms = new string[]
            {
                SecurityAlgorithms.EcdsaSha256
            };
        }
    }
}