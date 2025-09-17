namespace Testlemon.Core.Models
{
    public class ValidatorResult
    {
        public required Validator Validator { get; set; }

        public required IValidationResult Result { get; set; }
    }
}