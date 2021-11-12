using NzCovidPass.Core.Cbor;

namespace NzCovidPass.Core
{
    public class PassVerifierResult
    {
        private CborWebToken? _token;
        private List<FailureReason>? _failureReasons;
        private bool _failCalled;
        private bool _succeedCalled;

        internal PassVerifierResult()
        {
        }

        public bool HasSucceeded => !_failCalled && _succeedCalled && _token != null;

        public bool HasFailed => _failCalled;
        
        public IEnumerable<FailureReason> FailureReasons => 
            (IEnumerable<FailureReason>?) _failureReasons ?? Array.Empty<FailureReason>();

        public CborWebToken? Token => _token;

        public void Fail() 
        {
            _failCalled = true;
        }

        public void Fail(FailureReason failureReason)
        {
            ArgumentNullException.ThrowIfNull(failureReason);

            Fail();

            if (_failureReasons == null)
            {
                _failureReasons = new List<FailureReason>();
            }

            _failureReasons.Add(failureReason);
        }

        public void Succeed(CborWebToken token)
        {
            _succeedCalled = true;
            _token = token;
        }

        public override string ToString() 
        {
            if (HasSucceeded)
            {
                return $"{nameof(PassVerifierResult)}(Succeeded = {HasSucceeded})";
            }

            if (HasFailed)
            {
                return $"{nameof(PassVerifierResult)}(Failed = {HasFailed}, Reasons = [ {string.Join(", ", FailureReasons)} ])";
            }

            return $"{nameof(PassVerifierResult)}(Succeeded = {HasSucceeded}, Failed = {HasFailed})";
        }

        public readonly record struct FailureReason(string code, string message);

        internal static FailureReason PrefixValidationFailed => new(nameof(PrefixValidationFailed), "Prefix validation failed.");
        internal static FailureReason VersionValidationFailed => new(nameof(VersionValidationFailed), "Version validation failed.");
        internal static FailureReason TokenReadFailed => new(nameof(TokenReadFailed), "Token read failed.");
        internal static FailureReason IssuerValidationFailed => new(nameof(IssuerValidationFailed), "Issuer validation failed.");
    }
}