using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Encapsulates details of the token validation process.
    /// </summary>
    public class CborWebTokenValidatorContext : ValidationContext
    {
        private readonly CborWebToken _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="CborWebTokenValidatorContext" /> class.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        public CborWebTokenValidatorContext(CborWebToken token)
        {
            _token = Requires.NotNull(token);
        }

        /// <summary>
        /// Gets the token to validate.
        /// </summary>
        public CborWebToken Token => _token;

        /// <summary>
        /// Key identifier validation failure reason.
        /// </summary>
        public static FailureReason KeyIdValidationFailed => new(nameof(KeyIdValidationFailed), "Key ID validation failed.");

        /// <summary>
        /// Token identifier validation failure reason.
        /// </summary>
        public static FailureReason TokenIdValidationFailed => new(nameof(TokenIdValidationFailed), "Token ID validation failed.");

        /// <summary>
        /// Algorithm validation failure reason.
        /// </summary>
        public static FailureReason AlgorithmValidationFailed => new(nameof(AlgorithmValidationFailed), "Algorithm validation failed.");

        /// <summary>
        /// Issuer validation failure reason.
        /// </summary>
        public static FailureReason IssuerValidationFailed => new(nameof(IssuerValidationFailed), "Issuer validation failed.");

        /// <summary>
        /// Lifetime validation failure reason.
        /// </summary>
        public static FailureReason LifetimeValidationFailed => new(nameof(LifetimeValidationFailed), "Lifetime validation failed.");

        /// <summary>
        /// Failed verification key retrieval failure reason.
        /// </summary>
        public static FailureReason VerificationKeyRetrievalFailed => new(nameof(VerificationKeyRetrievalFailed), "Verification key retrieval failed.");

        /// <summary>
        /// Signature validation failure reason.
        /// </summary>
        public static FailureReason SignatureValidationFailed => new(nameof(SignatureValidationFailed), "Signature validation failed.");
    }
}
