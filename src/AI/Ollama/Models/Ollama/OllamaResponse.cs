using System.Text.Json.Serialization;

namespace OllamaClient.Models.Ollama
{
    public class OllamaResponse
    {
        [JsonPropertyName("response")]
        public required string Response { get; set; }
    }
}