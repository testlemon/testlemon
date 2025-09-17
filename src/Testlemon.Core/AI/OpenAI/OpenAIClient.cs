using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenAIClient.Models;

namespace Testlemon.OpenAIClient
{
    public class OpenAIClient
    {
        public const string OPEN_AI_ENDPOINT = nameof(OPEN_AI_ENDPOINT);
        public const string OPEN_AI_APIKEY = nameof(OPEN_AI_APIKEY);
        public const string OPEN_AI_MAX_TOKENS = nameof(OPEN_AI_MAX_TOKENS);

        private readonly HttpClient _httpClient;
        private readonly int _maxTokens;

        public OpenAIClient(string endpoint, string apiKey, int maxTokens)
        {
            _httpClient = new() { BaseAddress = new Uri(endpoint) };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _maxTokens = maxTokens;
        }

        public async Task<string> GetCompletionAsync(string model, string prompt)
        {
            var request = new ChatRequest
            {
                Model = model,
                Messages = [new() { Role = "user", Content = prompt }],
                MaxTokens = _maxTokens
            };

            var requestJson = JsonSerializer.Serialize(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            Console.WriteLine($"OpenAI request: {requestJson}");

            var message = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseJson);
            return chatResponse?.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;
        }
    }
}