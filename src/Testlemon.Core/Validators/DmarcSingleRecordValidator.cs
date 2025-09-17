using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class DmarcSingleRecordValidator : DmarcValidator, IValidator
    {
        public new string Name => "dns-dmarc-single-record";

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

            var result = new ValidationResult {
                IsSuccessful = records.Count() == 1,
                Items = records.Select(x => new ValidationResult { IsSuccessful = true, Value = x })
            };

            return result;
        }
    }
}