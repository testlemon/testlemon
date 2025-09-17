using DnsClient;

namespace Testlemon.Core.Validators
{
    public abstract class DKIMValidator : DNSValidator
    {
        const string DKIM_REQUIRED_PARAMETER = "p=";
        private static readonly string[] dkimSelectors = ["selector1", "selector2", "selector", "default"];

        protected static async Task<IEnumerable<string>> GetDKIMRecordsAsync(string url)
        {
            var rootDomain = GetRootDomain(url);
            if (string.IsNullOrWhiteSpace(rootDomain))
            {
                return [];
            }

            var dkimRecords = new List<string>();
            foreach (var selector in dkimSelectors)
            {
                var name = $"{selector}._domainkey.{rootDomain}";
                var dnsResponse = await GetDnsResponseAsync(name, QueryType.TXT);
                var records = dnsResponse.Answers.TxtRecords();
                var foundRecords = records.SelectMany(x => x.Text).Where(x => x.Contains(DKIM_REQUIRED_PARAMETER));
                dkimRecords.AddRange(foundRecords);
            }

            return dkimRecords;
        }
    }
}