using System.Text.Json.Serialization;

namespace Testlemon.Core.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContextType
    {
        Variable = 0, //default
        Secret
    }
}