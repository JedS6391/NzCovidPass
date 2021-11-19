using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NzCovidPass.Function
{
    public class VerifyPassRequest
    {
        [Required]
        [JsonPropertyName("passPayload")]
        public string PassPayload { get; set; }

        public bool IsValid()
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();

            return Validator.TryValidateObject(this, context, results, validateAllProperties: true);
        }
    }
}
