namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Contains claims about the subject(s) of a <see cref="VerifiableCredential{TCredential}" />.
    /// </summary>
    /// <remarks>
    /// <see href="https://w3c.github.io/vc-data-model/#credential-subject" />
    /// </remarks>
    public interface ICredentialSubject
    {
        /// <summary>
        /// Gets the JSON-LD context property value associated with the credential type.
        /// </summary>
        /// <remarks>
        /// This context must be present in the <see cref="VerifiableCredential{TCredential}.Context" /> collection.
        /// </remarks>
        string Context { get; }

        /// <summary>
        /// Gets the type property value associated with the credential type.
        /// </summary>
        /// <remarks>
        /// This type must be present in the <see cref="VerifiableCredential{TCredential}.Type" /> collection.
        /// </remarks>
        string Type { get; }
    }
}
