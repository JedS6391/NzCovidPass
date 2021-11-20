using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cwt
{
    /// <summary>
    /// Encapsulates details of the token validation process.
    /// </summary>
    public class CwtSecurityTokenValidatorContext : ValidationContext
    {
        private readonly CwtSecurityToken _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="CwtSecurityTokenValidatorContext" /> class.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        public CwtSecurityTokenValidatorContext(CwtSecurityToken token)
        {
            _token = Requires.NotNull(token);
        }

        /// <summary>
        /// Gets the token to validate.
        /// </summary>
        public CwtSecurityToken Token => _token;

        /// <summary>
        /// Key identifier validation failure reason.
        /// </summary>
        public static FailureReason KeyIdValidationFailed =>
            new(nameof(KeyIdValidationFailed), "Key ID (`kid`) parameter could not be found in CWT header.");

        /// <summary>
        /// Token identifier validation failure reason.
        /// </summary>
        public static FailureReason TokenIdValidationFailed =>
            new(nameof(TokenIdValidationFailed), "Token ID (mapped `cti`) parameter could not be found in CWT payload.");

        /// <summary>
        /// Algorithm validation failure reason.
        /// </summary>
        public static FailureReason AlgorithmValidationFailed(IEnumerable<string> validAlgorithms) =>
            new(nameof(AlgorithmValidationFailed),
                $"Algorithm (`alg`) parameter could not be found in CWT header or has an unexpected value [Valid algorithms = {string.Join(", ", validAlgorithms)}].");

        /// <summary>
        /// Issuer validation failure reason.
        /// </summary>
        public static FailureReason IssuerValidationFailed(IEnumerable<string> validIssuers) =>
            new(nameof(IssuerValidationFailed),
                $"Issuer (`iss`) parameter could not be found in CWT payload or has an unexpected value [Valid issuers = {string.Join(", ", validIssuers)}].");

        /// <summary>
        /// Lifetime validation failure reason.
        /// </summary>
        public static FailureReason LifetimeValidationFailed =>
            new(nameof(LifetimeValidationFailed), "Lifetime validation failed due to an inconsistency between not before parameter (`nbf`) and expiry parameter (`exp`).");

        /// <summary>
        /// Not before validation failure reason.
        /// </summary>
        public static FailureReason NotBeforeValidationFailed =>
            new(nameof(NotBeforeValidationFailed), "Not before (`nbf`) parameter could not be found in CWT payload or has an unexpected value.");

        /// <summary>
        /// Expiry validation failure reason.
        /// </summary>
        public static FailureReason ExpiryValidationFailed =>
            new(nameof(ExpiryValidationFailed), "Expiry (`exp`) parameter could not be found in CWT payload or has an unexpected value.");

        /// <summary>
        /// Failed verification key retrieval failure reason.
        /// </summary>
        public static FailureReason VerificationKeyRetrievalFailed =>
            new(nameof(VerificationKeyRetrievalFailed),
                "Verification key retrieval failed. This could be caused by DID document resolution failing due to a network error/invalid URL or the retrieved document not containing the expected assertion/verification method.");

        /// <summary>
        /// Signature validation failure reason.
        /// </summary>
        public static FailureReason SignatureValidationFailed => new(nameof(SignatureValidationFailed), "Signature validation failed.");

        /// <summary>
        /// Credential validation failure reason.
        /// </summary>
        public static FailureReason CredentialValidationFailed => new(nameof(CredentialValidationFailed), "Credential validation failed.");

        /// <summary>
        /// Credential context validation failure reason.
        /// </summary>
        public static FailureReason CredentialContextValidationFailed(string baseContext, string credentialContext) =>
            new(nameof(CredentialContextValidationFailed), $"Credential context is missing an expected value [Base context = {baseContext}, Credential context = {credentialContext}]");

        /// <summary>
        /// Credential type validation failure reason.
        /// </summary>
        public static FailureReason CredentialTypeValidationFailed(string baseType, string credentialType) =>
            new(nameof(CredentialTypeValidationFailed), $"Credential type is missing an expected value [Base type = {baseType}, Credential type = {credentialType}]");
    }
}
