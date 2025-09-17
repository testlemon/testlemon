using DnsClient;

namespace Testlemon.Core.Validators
{
    public abstract class SPFValidator : DNSValidator
    {
        const string SPF_SELECTOR = "v=spf1";

        protected static async Task<IEnumerable<string>> GetSPFRecordsAsync(string url)
        {
            var domain = GetRootDomain(url);
            if (string.IsNullOrWhiteSpace(domain))
            {
                return [];
            }

            var dnsResponse = await GetDnsResponseAsync(domain, QueryType.TXT);
            var spfRecords = dnsResponse.Answers.TxtRecords().SelectMany(x => x.Text).Where(x => x.Contains(SPF_SELECTOR));
            return spfRecords;
        }
    }
}