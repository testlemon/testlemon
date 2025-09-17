using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class Entity
    {
        [JsonPropertyName("vcardArray")]
        public List<object> VcardArray { get; set; } // Can be further detailed if needed

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }

        [JsonPropertyName("objectClassName")]
        public string ObjectClassName { get; set; }

        [JsonPropertyName("remarks")]
        public List<Remark> Remarks { get; set; }

        [JsonPropertyName("events")]
        public List<Event> Events { get; set; }

        [JsonPropertyName("publicIds")]
        public List<PublicId> PublicIds { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("links")]
        public List<Link> Links { get; set; }

        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; }
    }
}