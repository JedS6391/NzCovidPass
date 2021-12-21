using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a verifiable credential.
    /// </summary>
    /// <remarks>
    /// <see href="https://www.w3.org/TR/vc-data-model/" />
    /// </remarks>
    public class VerifiableCredential<TCredential> : VerifiableCredential
        where TCredential : class, ICredentialSubject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifiableCredential{TCredential}" /> class.
        /// </summary>
        /// <param name="version">The version of the credential.</param>
        /// <param name="context">The JSON-LD context properties.</param>
        /// <param name="type">The verifiable credential type properties.</param>
        /// <param name="credentialSubject">An object representing claims about the subject of the credential.</param>
        public VerifiableCredential(
            string version,
            IReadOnlyList<string> context,
            IReadOnlyList<string> type,
            TCredential credentialSubject)
        {
            Version = Requires.NotNull(version);
            Context = Requires.NotNull(context);
            Type = Requires.NotNull(type);
            CredentialSubject = Requires.NotNull(credentialSubject);
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the JSON-LD contexts.
        /// </summary>
        public IReadOnlyList<string> Context { get; private set; }

        /// <summary>
        /// Gets the verifiable credential types.
        /// </summary>
        public IReadOnlyList<string> Type { get; private set; }

        /// <summary>
        /// Gets the details of the subject the pass belongs to.
        /// </summary>
        public TCredential CredentialSubject { get; private set; }
    }

    /// <summary>
    /// Represents a verifiable credential.
    /// </summary>
    /// <remarks>
    /// <see href="https://www.w3.org/TR/vc-data-model/" />
    /// </remarks>
    public abstract class VerifiableCredential
    {
        /// <summary>
        /// The JSON-LD context property value associated with the base verifiable credential structure.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/vc-data-model/#contexts" />
        /// </remarks>
        public const string BaseContext = "https://www.w3.org/2018/credentials/v1";

        /// <summary>
        /// The type property value associated with the base verifiable credential type.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.w3.org/TR/vc-data-model/#types" />
        /// </remarks>
        public const string BaseCredentialType = "VerifiableCredential";
    }
}
