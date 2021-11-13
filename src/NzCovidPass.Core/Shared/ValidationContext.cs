namespace NzCovidPass.Core.Shared
{
    /// <summary>
    /// A base class for managing validation context.
    /// </summary>
    public abstract class ValidationContext
    {
        private List<FailureReason>? _failureReasons;
        private bool _failCalled;
        private bool _succeedCalled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext" /> class.
        /// </summary>
        protected ValidationContext()
        {
        }

        /// <summary>
        /// Gets a value indicating whether validation has succeeded.
        /// </summary>
        public bool HasSucceeded => !_failCalled && _succeedCalled;

        /// <summary>
        /// Gets a value indicating whether validation has failed.
        /// </summary>
        public bool HasFailed => _failCalled;

        /// <summary>
        /// Gets the reasons why validation has failed.
        /// </summary>
        public IEnumerable<FailureReason> FailureReasons =>
            (IEnumerable<FailureReason>?) _failureReasons ?? Array.Empty<FailureReason>();

        /// <summary>
        /// Indicates that validation has failed for this context.
        /// </summary>
        /// <remarks>
        /// Calling this method will ensure that <see cref="HasSucceeded" /> will never return <see langword="true" />.
        /// </remarks>
        public virtual void Fail()
        {
            _failCalled = true;
        }

        /// <summary>
        /// Indicates that validation has failed for this context, with the provided <param ref="failureReason" />.
        /// </summary>
        /// <remarks>
        /// Calling this method will ensure that <see cref="HasSucceeded" /> will never return <see langword="true" />.
        /// </remarks>
        public virtual void Fail(FailureReason failureReason)
        {
            ArgumentNullException.ThrowIfNull(failureReason);

            Fail();

            if (_failureReasons is null)
            {
                _failureReasons = new List<FailureReason>();
            }

            _failureReasons.Add(failureReason);
        }

        /// <summary>
        /// Indicates that validation has succeeded for this context.
        /// </summary>
        public virtual void Succeed()
        {
            _succeedCalled = true;
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Represents the reason a <see cref="ValidationContext" /> has been marked as failed.
        /// </summary>
        /// <param name="Code">A unique code for the failure.</param>
        /// <param name="Message">A message describing the failure.</param>
        public readonly record struct FailureReason(string Code, string Message);
    }
}
