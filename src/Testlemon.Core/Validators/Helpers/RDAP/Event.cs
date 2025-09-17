using System.Text.Json.Serialization;

namespace Testlemon.Core.Validators.Helpers.RDAP
{
    public class Event
    {
        [JsonPropertyName("eventAction")]
        public string EventAction { get; set; }

        [JsonPropertyName("eventDate")]
        public DateTime EventDate { get; set; }
    }
}