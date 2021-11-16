namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// Defines the ability to validate <see cref="CwtSecurityToken" /> instances.
    /// </summary>
    public interface ICwtSecurityTokenValidator
    {
        /// <summary>
        /// Determines the validity of a <see cref="CwtSecurityToken" /> instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The token to validate is provided in <see cref="CwtSecurityTokenValidatorContext.Token" />.
        /// </para>
        /// <para>
        /// The provided <see cref="CwtSecurityTokenValidatorContext" /> will be updated to indicate whether validation succeeded or not.
        /// </para>
        /// </remarks>
        /// <param name="context">A context to manage details of the verification process.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the details of the verification process.</returns>
        Task ValidateTokenAsync(CwtSecurityTokenValidatorContext context);
    }
}
