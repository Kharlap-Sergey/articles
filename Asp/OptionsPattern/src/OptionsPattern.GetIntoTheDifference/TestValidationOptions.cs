using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace OptionsPattern.GetIntoTheDifference
{
    public class TestValidationOptions
    {
        [Required]
        public string DataAnatationProperty { get; set; }
        public string ValidationPredicateProperty { get; set; }
        public string ExternalValidatorProperty { get; set; }
    }

    public class TestValidationOptionsValidator : IValidateOptions<TestValidationOptions>
    {
        public ValidateOptionsResult Validate(string name, TestValidationOptions options)
        {
            if (options.ExternalValidatorProperty == null)
            {
                return ValidateOptionsResult.Fail("ExternalValidatorProperty is null");
            }
            return ValidateOptionsResult.Success;
        }
    }
}
