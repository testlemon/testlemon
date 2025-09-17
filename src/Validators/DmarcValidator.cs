using DnsClient;

namespace Testlemon.Core.Validators
{
    public abstract class DmarcValidator : DNSValidator
    {
        const string DMARC_DOMAIN_PREFIX = "_dmarc.";

        protected static async Task<IEnumerable<string>> GetDmarcRecordsAsync(string url)
        {
            var domain = GetRootDomain(url);
            if (string.IsNullOrWhiteSpace(domain))
            {
                return [];
            }

            var dnsResponse = await GetDnsResponseAsync($"{DMARC_DOMAIN_PREFIX}{domain}", QueryType.TXT);
            var dmarcRecords = dnsResponse.Answers.TxtRecords().SelectMany(x => x.Text);
            return dmarcRecords;
        }
    }
}