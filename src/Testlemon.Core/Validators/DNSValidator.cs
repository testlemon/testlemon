using System.Text.RegularExpressions;
using DnsClient;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class DNSValidator : DomainValidator, IValidator
    {
        public string Name => "dns-record-exists";
        const string DNS_RECORD_REGEX_PATTERN = "^(.*?):(.*):(.*)$";

        protected static async Task<IDnsQueryResponse> GetDnsResponseAsync(string domainName, QueryType queryType)
        {
            // Create a LookupClient instance
            var lookup = new LookupClient();

            // Query records
            var response = await lookup.QueryAsync(domainName, queryType);

            return response;
        }

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var rootDomain = GetRootDomain(test.Url);
            if (string.IsNullOrWhiteSpace(rootDomain))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to parse root domain from '{test?.Url}'"
                };
            }

            var match = Regex.Match(validator.Value, DNS_RECORD_REGEX_PATTERN);
            if (!match.Success)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected value format error. Format: {DNS_RECORD_REGEX_PATTERN}"
                };
            }

            var type = match.Groups[1].Value.Trim();
            var name = match.Groups[2].Value.Trim();
            var data = match.Groups[3].Value.Trim();

            if (!Enum.TryParse(type, true, out QueryType queryType))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to parse the DNS record type: '{type}'"
                };
            }

            var records = string.IsNullOrWhiteSpace(name) || name == "@"
                ? await GetDnsResponseAsync(rootDomain, queryType)
                : await GetDnsResponseAsync($"{name}.{rootDomain}", queryType);

            var matchedRecords = records.Answers.Select(x => x.ToString()).Where(x => x.Contains(data, StringComparison.OrdinalIgnoreCase));

            var result = new ValidationResult
            {
                IsSuccessful = matchedRecords.Any(),
                Items = matchedRecords.Select(x => new ValidationResult
                {
                    Value = x,
                    IsSuccessful = true
                })
            };

            return result;
        }
    }
}