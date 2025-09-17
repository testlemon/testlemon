using Testlemon.Core.Validators.Helpers;

namespace Testlemon.Core.Validators
{
    public abstract class WhoisValidator : DomainValidator
    {
        protected static async Task<string?> GetWhoisAsync(string url)
        {
            var domainName = GetRootDomain(url);
            if (string.IsNullOrWhiteSpace(domainName))
            {
                return null;
            }

            var whois = await Whois.QueryAsync(domainName);
            return whois;
        }
    }
}