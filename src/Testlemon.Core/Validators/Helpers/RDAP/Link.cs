using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class Link
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("rel")]
        public string Rel { get; set; }

        [JsonPropertyName("href")]
        public string Href { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}