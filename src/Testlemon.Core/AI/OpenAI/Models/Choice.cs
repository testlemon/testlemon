using System.Text.Json.Serialization;

namespace OpenAIClient.Models
{
    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public required ChatMessage Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public required string FinishReason { get; set; }
    }
}