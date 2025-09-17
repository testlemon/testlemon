namespace Testlemon.Core.Models
{
    public class ValidationResult : IValidationResult
    {
        public bool IsSuccessful { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IEnumerable<ValidationResult> Items { get; set; } = [];
    }
}