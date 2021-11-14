namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Defines the ability to validate <see cref="CborWebToken" /> instances.
    /// </summary>
    public interface ICborWebTokenValidator
    {
        /// <summary>
        /// Determines the validity of a <see cref="CborWebToken" /> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The token to validate is provided in <see cref="CborWebTokenValidatorContext.Token" />.
        /// </para>
        /// <para>
        /// The provided <see cref="CborWebTokenValidatorContext" /> will be updated to indicate whether validation succeeded or not.
        /// </para>
        /// </remarks>
        /// <param name="context">A context to manage details of the verification process.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the details of the verification process.</returns>
        Task ValidateTokenAsync(CborWebTokenValidatorContext context);
    }
}
