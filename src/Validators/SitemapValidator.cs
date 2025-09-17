using TurnerSoftware.SitemapTools;

namespace Testlemon.Core.Validators
{
    public abstract class SitemapValidator : DomainValidator
    {
        public static async Task<IEnumerable<SitemapFile>> GetSitemapEntries(string url)
        {
            var domainName = GetSubDomain(url);
            if (string.IsNullOrWhiteSpace(domainName))
            {
                return [];
            }

            var sitemapQuery = new SitemapQuery();
            var sitemapEntries = await sitemapQuery.GetAllSitemapsForDomainAsync(domainName);

            return sitemapEntries;
        }
    }
}