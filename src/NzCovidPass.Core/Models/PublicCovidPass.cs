using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a New Zealand COVID pass verifiable credential type.
    /// </summary>
    /// <remarks>
    /// <see href="https://nzcp.covid19.health.nz/#publiccovidpass" />
    /// </remarks>
    public class PublicCovidPass : ICredentialSubject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicCovidPass" /> class.
        /// </summary>
        /// <param name="givenName">The given name of the subject.</param>
        /// <param name="familyName">The family name of the subject.</param>
        /// /// <param name="dateOfBirth">The date of birth of the subject.</param>
        public PublicCovidPass(string givenName, string familyName, DateTimeOffset dateOfBirth)
        {
            GivenName = Requires.NotNull(givenName);
            // Family name is optional
            FamilyName = familyName;
            DateOfBirth = dateOfBirth;
        }

        /// <summary>
        /// Gets the given name of the subject.
        /// </summary>
        public string GivenName { get; private set; }

        /// <summary>
        /// Gets the family name of the subject.
        /// </summary>
        public string FamilyName { get; private set; }

        /// <summary>
        /// Gets the date of birth of the subject.
        /// </summary>
        public DateTimeOffset DateOfBirth { get; private set; }

        /// <summary>
        /// The JSON-LD context property value associated with the <c>PublicCovidPass</c> verifiable credential type.
        /// </summary>
        /// <remarks>
        /// <see href="https://nzcp.covid19.health.nz/#verifiable-credential-claim-structure" />
        /// </remarks>
        public string Context => "https://nzcp.covid19.health.nz/contexts/v1";

        /// <summary>
        /// The type property value associated with the <c>PublicCovidPass</c> verifiable credential type.
        /// </summary>
        /// <remarks>
        /// <see href="https://nzcp.covid19.health.nz/#publiccovidpass" />
        /// </remarks>
        public string Type => "PublicCovidPass";
    }
}
