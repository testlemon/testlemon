using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Testlemon.Core.Models;
using Testlemon.Core.Output.Models.XRay;

namespace Testlemon.Core.Output;

public class XRayOutputFormatter
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = {
            new ISO8601DateTimeConverter()
        },
    };

    public static string Format(CollectionResult result)
    {
        static string? firstNonEmpty(params string?[] strings) => strings.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        var tests = result.TestsResults.Select(x => new XRayTest()
        {
            Status = x.IsValid ? "PASSED" : "FAILED",
            Start = x.Response.CreatedDate.DateTime,
            Finish = x.Response.CreatedDate.Add(x.Response.TotalDuration).DateTime,
            Comment = string.Join(Environment.NewLine, x.Validators.Where(result => !result.Result.IsSuccessful).Select(result => result.Result.Message)),
            TestKey = GetMetadataValue(x.Test.Metadata, "test-key"),
            TestInfo = new XRayTestInfo
            {
                Type = "Generic",
                ProjectKey = GetMetadataValue(x.Test.Metadata, "project-key"),
                RequirementKeys = GetMetadataValue(x.Test.Metadata, "requirement-keys")?.Split(',').ToList(),
                Summary = firstNonEmpty(GetMetadataValue(x.Test.Metadata, "summary"), x.Test.Name, x.Test.Id, $"{x.Test.Method} {x.Test.Url}"),
                Definition = firstNonEmpty(GetMetadataValue(x.Test.Metadata, "definition"), x.Test.Name, x.Test.Id, $"{x.Test.Method} {x.Test.Url}")
            }
        }).ToList();

        var import = new XRayImport
        {
            Info = new XRayInfo
            {
                Description = firstNonEmpty(GetMetadataValue(result.Metadata, "execution-description"), result.Name),
                Summary = firstNonEmpty(GetMetadataValue(result.Metadata, "execution-summary"), result.Name),
                Revision = GetMetadataValue(result.Metadata, "revision"),
                Version = GetMetadataValue(result.Metadata, "version"),
                TestPlanKey = GetMetadataValue(result.Metadata, "test-plan-key"),
                TestEnvironments = GetMetadataValue(result.Metadata, "environments")?.Split(",").ToList(),
            },
            Tests = tests
        };

        var json = JsonSerializer.Serialize(import, _jsonSerializerOptions);
        return json;
    }

    private static string? GetMetadataValue(IEnumerable<Dictionary<string, string>> metadata, string key)
    {
        return metadata?.FirstOrDefault(dict => dict.ContainsKey(key))?.GetValueOrDefault(key);
    }
}
