using System.Text.RegularExpressions;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class DmarcStrictPolicyValidator : DmarcValidator, IValidator
    {
        public new string Name => "dns-dmarc-strict-policy";
        const string DMARC_STRICT_POLICY_PATTERN = @"(?<=^|;)\s*p=(reject|quarantine)(?=;|$)";

        public new async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            if (!bool.TryParse(validator.Value, out bool success))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected value should be either '{true}' or '{false}', but was {validator.Value}"
                };
            }

            var records = await GetDmarcRecordsAsync(test.Url);
            var condition = records.Any(x => Regex.Match(x, DMARC_STRICT_POLICY_PATTERN).Success);

            var result = new ValidationResult
            {
                IsSuccessful = success == condition,
                Items = records.Select(x => new ValidationResult
                {
                    IsSuccessful = Regex.Match(x, DMARC_STRICT_POLICY_PATTERN).Success,
                    Value = x
                })
            };

            return result;
        }
    }
}