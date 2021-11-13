namespace NzCovidPass.Core.Verification
{
    /// <summary>
    /// Represents the error that occurs when a verification key cannot be resolved.
    /// </summary>
    public class VerificationKeyNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationKeyNotFoundException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public VerificationKeyNotFoundException(string message)
            : base(message)
        {
        }
    }
}
