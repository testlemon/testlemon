using System.Text.Json;
using testlemon;
using Testlemon.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Testlemon.Core
{
    public class CollectionParser
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
        };

        public static bool TryParseJson(string input, out Collection? collection)
        {
            try
            {
                collection = JsonSerializer.Deserialize<Collection>(input, _jsonSerializerOptions);
                if (collection != null)
                    return true;

                return false;
            }
            catch (Exception)
            {
                collection = default;
                return false;
            }
        }

        public static bool TryParseYaml(string input, out Collection? collection)
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance) // Use camel case if needed
                    .Build();

                collection = deserializer.Deserialize<Collection>(input);
                if (collection != null)
                    return true;

                return false;
            }
            catch (Exception)
            {
                collection = default;
                return false;
            }
        }

        public static bool TryParseOpenApi(string source, string openApiSpec, out Collection? collection)
        {
            try
            {
                collection = OpenApiSpecToCollectionConverter.ConvertFromSpec(openApiSpec, source);
                return true;
            }
            catch (Exception ex)
            {
                collection = default;
                return false;
            }
        }
    }
}