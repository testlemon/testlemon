using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class ResponseTimeValidator : IHttpResponseValidator
    {
        public string Name => "response-time";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var actualDuration = response?.TotalDuration;
            var expectedDuration = validator.Value;

            if (!int.TryParse(expectedDuration, out var expectedDurationValue))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected duration invalid format. Integer expected, but was '{expectedDuration}'"
                };
            }

            if (actualDuration?.TotalMilliseconds > expectedDurationValue)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Value = $"{actualDuration}",
                    Message = $"Actual duration: '{actualDuration}' is longer than expected: '{expectedDurationValue}'"
                };
            }

            var result = new ValidationResult
            {
                IsSuccessful = true,
                Value = $"{actualDuration}"
            };

            return await Task.FromResult(result);
        }
    }
}