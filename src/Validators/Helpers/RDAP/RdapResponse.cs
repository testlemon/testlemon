using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class RdapResponse
    {
        [JsonPropertyName("rdapConformance")]
        public List<string> RdapConformance { get; set; }

        [JsonPropertyName("notices")]
        public List<Notice> Notices { get; set; }

        [JsonPropertyName("ldhName")]
        public string LdhName { get; set; }

        [JsonPropertyName("unicodeName")]
        public string UnicodeName { get; set; }

        [JsonPropertyName("nameservers")]
        public List<Nameserver> Nameservers { get; set; }

        [JsonPropertyName("publicIds")]
        public List<PublicId> PublicIds { get; set; }

        [JsonPropertyName("objectClassName")]
        public string ObjectClassName { get; set; }

        [JsonPropertyName("handle")]
        public string Handle { get; set; }

        [JsonPropertyName("status")]
        public List<string> Status { get; set; }

        [JsonPropertyName("events")]
        public List<Event> Events { get; set; }

        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; }

        [JsonPropertyName("links")]
        public List<Link> Links { get; set; }
    }
}