using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class PublicId
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
    }
}