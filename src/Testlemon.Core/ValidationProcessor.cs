using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;


namespace Testlemon.Core
{
    public class ValidationProcessor(IEnumerable<IValidator> validators)
    {
        private readonly IEnumerable<IValidator> _validators = validators;

        public IValidator GetValidator(Validator validation)
        {
            // find a validator from all registered
            var validator = _validators
                .FirstOrDefault(x => x.Name.Equals(validation.Name, StringComparison.OrdinalIgnoreCase))
                 ?? throw new NotImplementedException($"Validator: {validation.Name} is not implemented.");

            return validator;
        }

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validation)
        {
            // find a validator from all registered
            var validator = GetValidator(validation);

            // validate the request / response using defined validator
            var isValid = await validator.ValidateAsync(test, response, validation);

            return await Task.FromResult(isValid);
        }
    }
}