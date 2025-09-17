namespace Testlemon.Core.Models
{
    public class TestResult
    {
        public required Test Test { get; set; }

        public required Response? Response { get; set; }

        public bool IsValid => Validators.All(x => x.Result.IsSuccessful);

        public required IEnumerable<ValidatorResult> Validators { get; set; }
    }
}