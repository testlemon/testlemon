using System.Net.Http.Headers;
using System.Text.Json;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class RDAP
    {
        const string RDAP_SERVER = "https://rdap.org";

        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(RDAP_SERVER)
        };

        public RDAP()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/rdap+json"));
        }

        public async Task<RdapResponse?> GetDomainInfoAsync(string domain)
        {
            // Construct the request URL
            string requestUrl = $"/domain/{domain}";

            // Send the GET request
            var response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Read the response content as a string
            string responseData = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON response into a dynamic object
            return JsonSerializer.Deserialize<RdapResponse>(responseData);
        }
    }
}