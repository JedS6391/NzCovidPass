using System.Text.Json.Serialization;

namespace NzCovidPass.Core.Models
{
    /// <summary>
    /// Represents a New Zealand COVID pass verifiable credential type.
    /// </summary>
    /// <remarks>
    /// <see href="https://nzcp.covid19.health.nz/#publiccovidpass" />
    /// </remarks>
    public class PublicCovidPass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicCovidPass" /> class.
        /// </summary>
        /// <param name="givenName">The given name of the subject.</param>
        /// <param name="familyName">The family name of the subject.</param>
        /// /// <param name="dateOfBirth">The date of birth of the subject.</param>
        [JsonConstructor]
        public PublicCovidPass(string givenName, string familyName, string dateOfBirth)
        {
            GivenName = givenName;
            FamilyName = familyName;
            DateOfBirth = dateOfBirth;
        }

        /// <summary>
        /// Gets the given name of the subject.
        /// </summary>
        [JsonPropertyName("givenName")]
        [JsonInclude]
        public string GivenName { get; private set; }

        /// <summary>
        /// Gets the family name of the subject.
        /// </summary>
        [JsonPropertyName("familyName")]
        [JsonInclude]
        public string FamilyName { get; private set; }

        /// <summary>
        /// Gets the date of birth of the subject.
        /// </summary>
        [JsonPropertyName("dob")]
        [JsonInclude]
        public string DateOfBirth { get; private set; }
    }
}
