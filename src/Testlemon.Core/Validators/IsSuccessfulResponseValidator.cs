using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class IsSuccessfulResponseValidator : IHttpResponseValidator
    {
        public string Name => "is-successful";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            if (!bool.TryParse(validator.Value, out bool success))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected value should be either '{true}' or '{false}', but was {validator.Value}"
                };
            }

            var result = new ValidationResult
            {
                IsSuccessful = response?.IsSuccessStatusCode == success,
                Value = $"{response?.StatusCode}"
            };

            return await Task.FromResult(result);
        }
    }
}