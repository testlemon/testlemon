using System.Text.RegularExpressions;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Helpers;
using Testlemon.Core.Validators.Helpers.RDAP;

namespace Testlemon.Core.Validators
{
    public abstract class WhoisDomainExpirationDateValidator : WhoisValidator
    {
        const string DATE_PATTERN = @"(?i)expir[\w\s]*[:]\s*([\d\-.:T\sZ]+)";

        private readonly RDAP _rdap = new();

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var expectedDate = ValidatorDateParser.ParseExpectedDate(validator.Value);
            if (expectedDate == null)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected date invalid format. Value: {validator.Value}"
                };
            }

            var date = await GetDomainExpirationDate(test.Url);
            if (date == null)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to get certificate expiration date from whois."
                };
            }

            var result = CompareDates(expectedDate.Value, date.Value);

            return new ValidationResult
            {
                IsSuccessful = result,
                Value = date.Value.ToString(),
            };
        }

        protected abstract bool CompareDates(DateTime expectedDate, DateTime domainExpirationDate);

        private async Task<DateTime?> GetDomainExpirationDate(string url)
        {
            var domain = GetRootDomain(url);
            if (string.IsNullOrWhiteSpace(domain))
            {
                return null;
            }

            // Try to get expiration date from RDAP service
            var rdap = await _rdap.GetDomainInfoAsync(domain);
            if (rdap != null)
            {
                var expirationDate = rdap.Events.Single(x => x.EventAction.Equals("expiration", StringComparison.OrdinalIgnoreCase));
                if (expirationDate != null)
                {
                    return expirationDate.EventDate;
                }
            }

            // If no results from RDAP, get from whois.
            var whois = await GetWhoisAsync(url);
            if (string.IsNullOrWhiteSpace(whois))
            {
                return null;
            }

            var match = Regex.Match(whois, DATE_PATTERN, RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out DateTime date))
            {
                return date;
            }

            return null;
        }
    }
}