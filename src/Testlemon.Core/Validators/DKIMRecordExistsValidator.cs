using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class DKIMRecordExistsValidator : DKIMValidator, IValidator
    {
        public new string Name => "dns-dkim-exists";

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

            var records = await GetDKIMRecordsAsync(test.Url);

            var result = new ValidationResult
            {
                IsSuccessful = success == records.Any(),
                Items = records.Select(x => new ValidationResult
                {
                    IsSuccessful = true,
                    Value = x
                })
            };

            return result;
        }
    }
}