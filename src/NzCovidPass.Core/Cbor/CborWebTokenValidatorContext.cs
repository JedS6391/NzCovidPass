using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cbor
{
    public class CborWebTokenValidatorContext : Context
    {
        private readonly CborWebToken _token;

        public CborWebTokenValidatorContext(CborWebToken token)
        {
            _token = token;
        }

        public CborWebToken Token => _token;

        public static FailureReason KeyIdValidationFailed => new(nameof(KeyIdValidationFailed), "Key ID validation failed.");
        public static FailureReason TokenIdValidationFailed => new(nameof(TokenIdValidationFailed), "Token ID validation failed.");
        public static FailureReason AlgorithmValidationFailed => new(nameof(AlgorithmValidationFailed), "Algorithm validation failed.");
        public static FailureReason IssuerValidationFailed => new(nameof(IssuerValidationFailed), "Issuer validation failed.");
        public static FailureReason LifetimeValidationFailed => new(nameof(LifetimeValidationFailed), "Lifetime validation failed.");
        public static FailureReason SignatureValidationFailed => new(nameof(SignatureValidationFailed), "Signature validation failed.");
    }
}