using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a Decentralized Identifier (DID) document, as described by <see href="https://www.w3.org/TR/did-core/#did-documents" />.
    /// </summary>
    public class DecentralizedIdentifierDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecentralizedIdentifierDocument" /> class.
        /// </summary>
        /// <param name="id">The decentralized identifier.</param>
        /// <param name="contexts">The JSON-LD contexts.</param>
        /// <param name="verificationMethods">The verification methods.</param>
        /// <param name="assertionMethods">The assertion methods.</param>
        [JsonConstructor]
        public DecentralizedIdentifierDocument(
            string id,
            IReadOnlyList<string> contexts,
            IReadOnlyList<VerificationMethod> verificationMethods,
            IReadOnlyList<string> assertionMethods)
        {
            Id = id;
            Contexts = contexts;
            VerificationMethods = verificationMethods;
            AssertionMethods = assertionMethods;
        }

        /// <summary>
        /// Gets the decentralized identifier.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/did-core/#dfn-decentralized-identifiers" />
        /// </remarks>
        [JsonPropertyName("id")]
        [JsonInclude]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the JSON-LD context.
        /// </summary>
        [JsonPropertyName("@context")]
        [JsonInclude]
        [JsonConverter(typeof(ContextJsonConverter))]
        public IReadOnlyList<string> Contexts { get; private set; }

        /// <summary>
        /// Gets the verification methods.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/did-core/#verification-methods" />
        /// </remarks>
        [JsonPropertyName("verificationMethod")]
        [JsonInclude]
        public IReadOnlyList<VerificationMethod> VerificationMethods { get; private set; }

        /// <summary>
        /// Gets the assertion methods.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/did-core/#assertion" />
        /// </remarks>
        [JsonPropertyName("assertionMethod")]
        [JsonInclude]
        public IReadOnlyList<string> AssertionMethods { get; private set; }

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(DecentralizedIdentifierDocument)}(id = {Id})";

        /// <summary>
        /// Represents a decentralized identifier document verification method.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/did-core/#verification-methods" />
        /// </remarks>
        public class VerificationMethod
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VerificationMethod" /> class.
            /// </summary>
            /// <param name="id">The verification method identifier.</param>
            /// <param name="controller">The verification method controller.</param>
            /// <param name="type">The verification method type.</param>
            /// <param name="publicKey">The verification method public key.</param>
            [JsonConstructor]
            public VerificationMethod(string id, string controller, string type, JsonWebKey publicKey)
            {
                Id = id;
                Controller = controller;
                Type = type;
                PublicKey = publicKey;
            }

            /// <summary>
            /// Gets the verification method identifier.
            /// </summary>
            [JsonPropertyName("id")]
            [JsonInclude]
            public string Id { get; private set; }

            /// <summary>
            /// Gets the verification method controller.
            /// </summary>
            [JsonPropertyName("controller")]
            [JsonInclude]
            public string Controller { get; private set; }

            /// <summary>
            /// Gets the verification method type.
            /// </summary>
            [JsonPropertyName("type")]
            [JsonInclude]
            public string Type { get; private set; }

            /// <summary>
            /// Gets the verification method public key as a <see cref="JsonWebKey" />.
            /// </summary>
            [JsonPropertyName("publicKeyJwk")]
            [JsonInclude]
            public JsonWebKey PublicKey { get; private set; }
        }
    }
}
