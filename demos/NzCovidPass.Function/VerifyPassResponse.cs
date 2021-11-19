using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NzCovidPass.Function
{
    public abstract class VerifyPassResponse
    {
        public abstract bool IsValid { get; }
    }

    public sealed class ValidPassResponse : VerifyPassResponse
    {
        [JsonPropertyName("isValid")]
        public override bool IsValid => true;

        [JsonPropertyName("givenName")]
        public string GivenName { get; set; }

        [JsonPropertyName("familyName")]
        public string FamilyName { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateTimeOffset DateOfBirth { get; set; }

        [JsonPropertyName("expiresOn")]
        public DateTimeOffset ExpiresOn { get; set; }
    }

    public sealed class InvalidPassResponse : VerifyPassResponse
    {
        [JsonPropertyName("isValid")]
        public override bool IsValid => false;

        [JsonPropertyName("failureCodes")]
        public IEnumerable<string> FailureCodes { get; set; }
    }
}
