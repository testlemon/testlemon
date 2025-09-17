using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class Nameserver
    {
        [JsonPropertyName("ldhName")]
        public string LdhName { get; set; }

        [JsonPropertyName("unicodeName")]
        public string UnicodeName { get; set; }

        [JsonPropertyName("objectClassName")]
        public string ObjectClassName { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("status")]
        public List<string> Status { get; set; }
    }
}