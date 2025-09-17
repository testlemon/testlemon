using System.Text.Json.Serialization;

namespace Testlemon.Core.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Patch,
        Options,
        Head,
        Trace,
    }
}