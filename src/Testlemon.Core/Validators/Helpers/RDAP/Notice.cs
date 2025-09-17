using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class Notice
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public List<string> Description { get; set; }

        [JsonPropertyName("links")]
        public List<Link> Links { get; set; }
    }
}