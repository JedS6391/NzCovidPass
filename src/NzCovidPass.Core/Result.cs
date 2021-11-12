namespace NzCovidPass.Core.Shared
{
    public abstract class Context
    {
        private List<FailureReason>? _failureReasons;
        private bool _failCalled;
        private bool _succeedCalled;

        protected Context()
        {
        }

        public bool HasSucceeded => !_failCalled && _succeedCalled;

        public bool HasFailed => _failCalled;
        
        public IEnumerable<FailureReason> FailureReasons => 
            (IEnumerable<FailureReason>?) _failureReasons ?? Array.Empty<FailureReason>(); 

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

        public void Succeed()
        {
            _succeedCalled = true;
        }

        public readonly record struct FailureReason(string code, string message);
    }
}