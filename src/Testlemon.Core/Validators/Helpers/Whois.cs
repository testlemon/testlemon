using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Testlemon.Core.Validators.Helpers
{
    public class Whois
    {
        const int WHOIS_SERVER_DEFAULT_PORT_NUMBER = 43;
        const string WHOIS_ROOT_SERVER = "whois.iana.org";

        public static async Task<string?> QueryAsync(string domain)
        {
            var whoisInfo = await LookupAsync(domain, WHOIS_ROOT_SERVER);
            if (string.IsNullOrWhiteSpace(whoisInfo))
            {
                return null;
            }

            var match = Regex.Match(whoisInfo, @"whois:\s*(.*)", RegexOptions.IgnoreCase);
            if (!match.Success || match.Groups.Count < 2 || string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
                return null;
            }

            var server = match.Groups[1].Value;
            var info = await LookupAsync(domain, server);
            return info;
        }

        private static async Task<string> LookupAsync(string domainName, string whoisServer)
        {
            using TcpClient client = new(whoisServer, WHOIS_SERVER_DEFAULT_PORT_NUMBER);
            using StreamWriter writer = new(client.GetStream());
            using StreamReader reader = new(client.GetStream());

            writer.WriteLine(domainName + "\r\n");
            writer.Flush();

            // Read the WHOIS response
            var result = await reader.ReadToEndAsync();

            return result;
        }
    }
}