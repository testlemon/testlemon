namespace Testlemon.Core.Models
{
    public interface IValidationResult
    {
        public bool IsSuccessful { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
        public IEnumerable<ValidationResult> Items { get; set; }
    }
}