using Testlemon.Core.Models;

namespace Testlemon.Core.Validators.Abstract
{
    public interface IValidator
    {
        public string Name { get; }

        Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator);
    }
}