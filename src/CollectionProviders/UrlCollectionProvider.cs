namespace Testlemon.Core.CollectionProviders
{
    internal class UrlCollectionProvider(string path, Dictionary<string, string> headers) : ICollectionProvider
    {
        private readonly string _path = path;
        private readonly Dictionary<string, string> _headers = headers;

        public async Task<IEnumerable<string>> GetAsync()
        {
            if (!Uri.IsWellFormedUriString(_path, UriKind.Absolute))
                throw new UriFormatException($"The following url is not correct: {_path}");

            var httpClient = new HttpClient();

            if (_headers != null && _headers.Any())
            {
                foreach (var header in _headers)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var response = await httpClient.GetAsync(new Uri(_path));
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException($"The response from URL was not successful. Try again or change the request URL. Url: {_path}");
            }

            var content = await response.Content.ReadAsStringAsync();

            return [content];
        }
    }
}