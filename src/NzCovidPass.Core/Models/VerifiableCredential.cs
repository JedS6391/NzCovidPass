using System.Text.Json.Serialization;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a verifiable credential.
    /// </summary>
    /// <remarks>
    /// <see href="https://www.w3.org/TR/vc-data-model/" />
    /// </remarks>
    public class VerifiableCredential<TCredential>
        where TCredential : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiableCredential{TCredential}" /> class.
        /// </summary>
        /// <param name="version">The version of the credential.</param>
        /// <param name="context">The JSON-LD context properties.</param>
        /// <param name="type">The verifiable credential type properties.</param>
        /// <param name="credentialSubject">An object representing claims about the subject of the credential.</param>
        [JsonConstructor]
        public VerifiableCredential(
            string version,
            IReadOnlyList<string> context,
            IReadOnlyList<string> type,
            TCredential credentialSubject)
        {
            Version = Requires.NotNull(version);
            Context = Requires.NotNull(context);
            Type = Requires.NotNull(type);
            CredentialSubject = Requires.NotNull(credentialSubject);
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; private set; }

        /// <summary>
        /// Gets the JSON-LD contexts.
        /// </summary>
        [JsonPropertyName("@context")]
        [JsonInclude]
        [JsonConverter(typeof(ContextJsonConverter))]
        public IReadOnlyList<string> Context { get; private set; }

        /// <summary>
        /// Gets the verifiable credential types.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonInclude]
        public IReadOnlyList<string> Type { get; private set; }

        /// <summary>
        /// Gets the details of the subject the pass belongs to.
        /// </summary>
        [JsonPropertyName("credentialSubject")]
        [JsonInclude]
        public TCredential CredentialSubject { get; private set; }
    }
}
