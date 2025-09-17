using System.Text.Json.Serialization;

namespace Testlemon.Core.Output.Models.XRay
{
    public class XRayTest
    {
        public XRayTestInfo? TestInfo { get; set; }
        
        public string? TestKey { get; set; }
        
        [JsonConverter(typeof(ISO8601DateTimeConverter))]
        public DateTime Start { get; set; }
        
        [JsonConverter(typeof(ISO8601DateTimeConverter))]
        public DateTime Finish { get; set; }
        
        public string? Comment { get; set; }
        
        public string? Status { get; set; }
    }
}