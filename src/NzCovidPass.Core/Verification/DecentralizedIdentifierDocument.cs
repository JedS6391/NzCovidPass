using System.Text.Json;
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
        [JsonConverter(typeof(ContextJsonConverter))]
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


        private class ContextJsonConverter : JsonConverter<IReadOnlyList<string>>
        {
            public override IReadOnlyList<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                reader.TokenType switch
                {
                    JsonTokenType.StartArray => JsonSerializer.Deserialize<List<string>>(ref reader, options),
                    JsonTokenType.String => new List<string>() { reader.GetString() },
                    _ => throw new JsonException("Unexpected JSON data for context."),
                };

            public override void Write(Utf8JsonWriter writer, IReadOnlyList<string> value, JsonSerializerOptions options) =>
                throw new NotImplementedException();            
        }
    }
}
