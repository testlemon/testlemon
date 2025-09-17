namespace Testlemon.Core.Validators
{
    public abstract class DomainValidator
    {
        protected static string? GetRootDomain(string url)
        {
            // Parse the URL
            if(!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            var host = new Uri(url).Host;
            
            // Split the host into parts
            string[] hostParts = host.Split('.');
            
            if (hostParts.Length >= 2)
            {
                // Get the last two parts for the root domain
                string rootDomain = $"{hostParts[^2]}.{hostParts[^1]}";
                return rootDomain;
            }
            
            // Fallback if for some reason it's not a valid domain
            return host;
        }

        protected static string? GetSubDomain(string url)
        {
            // Parse the URL
            if(!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            return new Uri(url).Host;
        }
    }
}