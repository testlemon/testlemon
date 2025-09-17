using System.Text.Json.Serialization;

namespace OpenAIClient.Models
{
    public class ChatResponse
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("object")]
        public required string Object { get; set; }

        [JsonPropertyName("created")]
        public int Created { get; set; }

        [JsonPropertyName("choices")]
        public required List<Choice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public required Usage Usage { get; set; }
    }
}