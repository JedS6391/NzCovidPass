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

        public virtual void Fail()
        {
            _failCalled = true;
        }

        public virtual void Fail(FailureReason failureReason)
        {
            ArgumentNullException.ThrowIfNull(failureReason);

            Fail();

            if (_failureReasons == null)
            {
                _failureReasons = new List<FailureReason>();
            }

            _failureReasons.Add(failureReason);
        }

        public virtual void Succeed()
        {
            _succeedCalled = true;
        }

        public override string ToString()
        {
            if (HasSucceeded)
            {
                return $"{GetType().Name}(Succeeded = {HasSucceeded})";
            }

            if (HasFailed)
            {
                return $"{GetType().Name}(Failed = {HasFailed}, Reasons = [ {string.Join(", ", FailureReasons)} ])";
            }

            return $"{GetType().Name}(Succeeded = {HasSucceeded}, Failed = {HasFailed})";
        }

        public readonly record struct FailureReason(string Code, string Message);
    }
}
