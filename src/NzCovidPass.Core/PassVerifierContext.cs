using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core
{
    public class PassVerifierContext : Context
    {
        private CborWebToken? _token;

        public PassVerifierContext()
        {
        }

        public CborWebToken? Token => _token;

        public void Succeed(CborWebToken token)
        {
            base.Succeed();
            
            _token = token;
        }

        public static FailureReason InvalidPassComponents => new(nameof(InvalidPassComponents), "Invalid pass components.");
        public static FailureReason InvalidPassPayload => new(nameof(InvalidPassPayload), "Invalid pass payload.");
        public static FailureReason PrefixValidationFailed => new(nameof(PrefixValidationFailed), "Prefix validation failed.");
        public static FailureReason VersionValidationFailed => new(nameof(VersionValidationFailed), "Version validation failed.");
        public static FailureReason TokenReadFailed => new(nameof(TokenReadFailed), "Token read failed.");
        public static FailureReason TokenValidationFailed => new(nameof(TokenValidationFailed), "Token validation failed.");
    }
}