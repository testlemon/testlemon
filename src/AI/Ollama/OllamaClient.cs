using System.Net.Http.Json;
using OllamaClient.Models.Ollama;

namespace Testlemon.OllamaClient
{
    public class OllamaClient(string endpoint, uint timeoutInSeconds = 5 * 60)
    {
        public const string OLLAMA_ENDPOINT = nameof(OLLAMA_ENDPOINT);
        public const string OLLAMA_TIMEOUT = nameof(OLLAMA_TIMEOUT);
        private readonly HttpClient _httpClient = new()
        {
            BaseAddress = new Uri(endpoint),
            Timeout = TimeSpan.FromSeconds(timeoutInSeconds)
        };

        public async Task<string> GetCompletionAsync(string prompt, string model)
        {
            var body = new
            {
                model,
                prompt,
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", body);
            response.EnsureSuccessStatusCode();

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            return ollamaResponse?.Response ?? string.Empty;
        }
    }
}