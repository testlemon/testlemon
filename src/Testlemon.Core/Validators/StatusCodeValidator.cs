using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class StatusCodeValidator : IHttpResponseValidator
    {
        public string Name => "status-code";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var assertionStatusCode = validator.Value;

            var responseStatusCode = response?.StatusCode;
            var statusCode = responseStatusCode?.ToString();

            if (string.IsNullOrWhiteSpace(statusCode))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to get response status code."
                };
            }

            var result = new ValidationResult
            {
                IsSuccessful = statusCode.Equals(assertionStatusCode, StringComparison.OrdinalIgnoreCase),
                Value = statusCode
            };

            return await Task.FromResult(result);
        }
    }
}