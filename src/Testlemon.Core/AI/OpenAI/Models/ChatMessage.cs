using System.Text.Json.Serialization;

namespace OpenAIClient.Models
{
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}