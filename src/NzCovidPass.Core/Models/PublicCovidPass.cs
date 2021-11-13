using System.Text.Json.Serialization;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a New Zealand COVID pass verifiable credential type.
    /// </summary>
    /// <remarks>
    /// <see href="https://nzcp.covid19.health.nz/#publiccovidpass" />
    /// </remarks>
    public class PublicCovidPass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicCovidPass" /> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="contexts">The JSON-LD contexts.</param>
        /// <param name="types">The verifiable credential types.</param>
        /// <param name="details">The details of the subject the pass belongs to.</param>
        [JsonConstructor]
        public PublicCovidPass(
            string version,
            IReadOnlyList<string> contexts,
            IReadOnlyList<string> types,
            Credentials details)
        {
            Version = version;
            Contexts = contexts;
            Types = types;
            Details = details;
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
        public IReadOnlyList<string> Contexts { get; private set; }

        /// <summary>
        /// Gets the verifiable credential types.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonInclude]
        public IReadOnlyList<string> Types { get; private set; }

        /// <summary>
        /// Gets the details of the subject the pass belongs to.
        /// </summary>
        [JsonPropertyName("credentialSubject")]
        [JsonInclude]
        public Credentials Details { get; private set; }

        /// <summary>
        /// Represents the NZ Covid Pass credentials subject details.
        /// </summary>
        public class Credentials
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Credentials" /> class.
            /// </summary>
            /// <param name="givenName">The given name of the subject.</param>
            /// <param name="familyName">The family name of the subject.</param>
            /// /// <param name="dateOfBirth">The date of birth of the subject.</param>
            [JsonConstructor]
            public Credentials(string givenName, string familyName, string dateOfBirth)
            {
                GivenName = givenName;
                FamilyName = familyName;
                DateOfBirth = dateOfBirth;
            }

            /// <summary>
            /// Gets the given name of the subject.
            /// </summary>
            [JsonPropertyName("givenName")]
            [JsonInclude]
            public string GivenName { get; private set; }

            /// <summary>
            /// Gets the family name of the subject.
            /// </summary>
            [JsonPropertyName("familyName")]
            [JsonInclude]
            public string FamilyName { get; private set; }

            /// <summary>
            /// Gets the date of birth of the subject.
            /// </summary>
            [JsonPropertyName("dob")]
            [JsonInclude]
            public string DateOfBirth { get; private set; }
        }
    }
}
