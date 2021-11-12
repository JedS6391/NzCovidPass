using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Verification
{
    public class DecentralizedIdentifierDocument
    {
        [JsonPropertyName("id")]
        [JsonInclude]
        public string Id { get; private set; }

        [JsonPropertyName("@context")]
        [JsonInclude]
        public IReadOnlyList<string> Contexts { get; private set; }

        [JsonPropertyName("verificationMethod")]
        [JsonInclude]
        public IReadOnlyList<VerificationMethod> VerificationMethods { get; private set; }

        [JsonPropertyName("assertionMethod")]
        [JsonInclude]
        public IReadOnlyList<string> AssertionMethods { get; private set; }

        public override string ToString() => $"{nameof(DecentralizedIdentifierDocument)}(id = {Id})";
        
        public class VerificationMethod 
        {
            [JsonPropertyName("id")]
            [JsonInclude]
            public string Id { get; private set; }

            [JsonPropertyName("controller")]
            [JsonInclude]
            public string Controller { get; private set; }

            [JsonPropertyName("type")]
            [JsonInclude]
            public string Type { get; private set; }

            [JsonPropertyName("publicKeyJwk")]
            [JsonInclude]
            public JsonWebKey PublicKey { get; private set; }        
        }
    }
}
