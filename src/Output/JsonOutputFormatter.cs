using System.Text.Json;
using Testlemon.Core.Models;

namespace Testlemon.Core.Output;

public class JsonOutputFormatter
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string Format(CollectionResult result)
    {
        var json = JsonSerializer.Serialize(result, _jsonSerializerOptions);
        return json;
    }
}
