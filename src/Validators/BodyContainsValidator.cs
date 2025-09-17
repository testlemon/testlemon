using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class BodyContainsValidator : IHttpResponseValidator
    {
        public string Name => "body-contains";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var responseBody = response?.Body;
            var expectedValue = validator.Value;

            var isSuccessful = responseBody?.Contains(expectedValue, StringComparison.Ordinal) ?? false;

            var validationResult = new ValidationResult
            {
                IsSuccessful = isSuccessful,
                Value = responseBody ?? string.Empty
            };

            return await Task.FromResult(validationResult);
        }
    }
}