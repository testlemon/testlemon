using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class BodyEqualsValidator : IHttpResponseValidator
    {
        public string Name => "body-equals";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var responseBody = response?.Body;
            var expectedValue = validator.Value;

            var isSuccessful = responseBody?.Equals(expectedValue, StringComparison.OrdinalIgnoreCase) ?? false;
            
            var validationResult = new ValidationResult
            {
                IsSuccessful = isSuccessful,
                Value = responseBody ?? string.Empty
            };

            return await Task.FromResult(validationResult);
        }
    }
}