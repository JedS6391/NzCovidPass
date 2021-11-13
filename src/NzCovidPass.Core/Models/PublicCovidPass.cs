using System.Text.Json.Serialization;

namespace NzCovidPass.Core.Models
{
    public class PublicCovidPass
    {
        [JsonPropertyName("@context")]
        [JsonInclude]
        [JsonConverter(typeof(ContextJsonConverter))]
        public IReadOnlyList<string> Contexts { get; private set; }

        [JsonPropertyName("version")]
        [JsonInclude]
        public string Version { get; private set; }

        [JsonPropertyName("type")]
        [JsonInclude]
        public IReadOnlyList<string> Types { get; private set; }

        [JsonPropertyName("credentialSubject")]
        [JsonInclude]
        public Credentials Details { get; private set; }

        public class Credentials
        {
            [JsonPropertyName("givenName")]
            [JsonInclude]
            public string GivenName { get; private set; }

            [JsonPropertyName("familyName")]
            [JsonInclude]
            public string FamilyName { get; private set; }

            [JsonPropertyName("dob")]
            [JsonInclude]
            public string DateOfBirth { get; private set; }
        }
    }
}
