using Testlemon.Core.Models;
using Testlemon.Core.Validators.Helpers;

namespace Testlemon.Core.Validators
{
    public abstract class SSLExpirationDateValidator : SSLValidator
    {
        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Models.Validator validator)
        {
            var certificate = await GetSSLCertificate(test.Url);
            if (certificate == null)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Certification is not found. Url: {test.Url}"
                };
            }

            var expectedDate = ValidatorDateParser.ParseExpectedDate(validator.Value);
            if (expectedDate == null)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected date invalid format. Value: {validator.Value}"
                };
            }

            var compare = CompareDates(expectedDate.Value, certificate.NotAfter);

            return new ValidationResult
            {
                IsSuccessful = compare,
                Value = $"{certificate.NotAfter}"
            };
        }

        protected abstract bool CompareDates(DateTime expectedDate, DateTime cetificateDate);
    }
}