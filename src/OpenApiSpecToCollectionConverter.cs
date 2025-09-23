using System.Text.Json;
using System.Globalization;
using Microsoft.OpenApi.Readers;
using Testlemon.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using HttpMethod = Testlemon.Core.Models.HttpMethod;
using Microsoft.OpenApi.Any;

namespace testlemon
{
    /// <summary>
    /// A unified converter that can handle OpenAPI specifications in both YAML and JSON formats
    /// and convert them to Testlemon Collection format.
    /// </summary>
    public class OpenApiSpecToCollectionConverter
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        /// <summary>
        /// Converts an OpenAPI specification (YAML or JSON) to a Collection.
        /// </summary>
        /// <param name="openApiSpec">The OpenAPI specification content as a string</param>
        /// <param name="source">Optional source identifier for the specification</param>
        /// <returns>A Collection object representing the OpenAPI specification</returns>
        /// <exception cref="ArgumentException">Thrown when the input is invalid or parsing fails</exception>
        public static Collection ConvertFromSpec(string openApiSpec, string source = "")
        {
            if (string.IsNullOrWhiteSpace(openApiSpec))
                throw new ArgumentException("OpenAPI specification content cannot be null or empty.", nameof(openApiSpec));

            try
            {
                // Use Microsoft.OpenApi library for parsing both YAML and JSON
                var reader = new OpenApiStringReader();
                var document = reader.Read(openApiSpec, out var diagnostic);

                if (diagnostic?.Errors?.Count > 0)
                {
                    var errorMessages = string.Join("; ", diagnostic.Errors.Select(e => e.Message));
                    throw new ArgumentException($"Failed to parse OpenAPI specification: {errorMessages}");
                }

                return ConvertDocumentToCollection(document, source);
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ArgumentException($"Failed to process OpenAPI specification: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts an OpenAPI specification from a file path to a Collection.
        /// </summary>
        /// <param name="filePath">Path to the OpenAPI specification file</param>
        /// <param name="source">Optional source identifier</param>
        /// <returns>A Collection object representing the OpenAPI specification</returns>
        /// <exception cref="ArgumentException">Thrown when the file cannot be read or parsed</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        public static Collection ConvertFromFile(string filePath, string source = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"OpenAPI specification file not found: {filePath}");

            try
            {
                var content = File.ReadAllText(filePath);
                return ConvertToCollection(content, source ?? filePath);
            }
            catch (Exception ex) when (!(ex is ArgumentException) && !(ex is FileNotFoundException))
            {
                throw new ArgumentException($"Failed to read OpenAPI specification from file '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts a parsed OpenAPI document to a Collection.
        /// </summary>
        /// <param name="document">The parsed OpenAPI document</param>
        /// <param name="source">Source identifier</param>
        /// <returns>A Collection object</returns>
        private static Collection ConvertDocumentToCollection(Microsoft.OpenApi.Models.OpenApiDocument document, string source)
        {
            if (document.Servers == null || !document.Servers.Any())
                throw new ArgumentException("OpenAPI specification must contain at least one server.");

            var baseUrl = document.Servers.First().Url.TrimEnd('/');
            if (Uri.IsWellFormedUriString(baseUrl, UriKind.Relative) && !string.IsNullOrEmpty(source))
            {
                try
                {
                    var sourceUri = new Uri(source);
                    baseUrl = $"{sourceUri.Scheme}://{sourceUri.Host}{baseUrl}";
                }
                catch
                {
                    // If source parsing fails, use the base URL as-is
                }
            }

            var collection = new Collection
            {
                Name = document.Info?.Title ?? "OpenAPI Collection",
                BaseUrl = baseUrl,
                Metadata = CreateMetadata(document, source),
                Tests = CreateTests(document)
            };

            return collection;
        }

        /// <summary>
        /// Creates metadata for the collection from the OpenAPI document.
        /// </summary>
        /// <param name="document">The OpenAPI document</param>
        /// <param name="source">Source identifier</param>
        /// <returns>Collection metadata</returns>
        private static IEnumerable<Dictionary<string, string>> CreateMetadata(Microsoft.OpenApi.Models.OpenApiDocument document, string source)
        {
            var metadata = new List<Dictionary<string, string>>();

            if (!string.IsNullOrEmpty(source))
            {
                metadata.Add(new Dictionary<string, string> { ["source"] = source });
            }

            if (!string.IsNullOrEmpty(document.Info?.Version))
            {
                metadata.Add(new Dictionary<string, string> { ["version"] = document.Info.Version });
            }

            if (!string.IsNullOrEmpty(document.Info?.Description))
            {
                metadata.Add(new Dictionary<string, string> { ["description"] = document.Info.Description });
            }

            if (document.Info?.Contact != null)
            {
                if (!string.IsNullOrEmpty(document.Info.Contact.Email))
                {
                    metadata.Add(new Dictionary<string, string> { ["contact_email"] = document.Info.Contact.Email });
                }
                if (!string.IsNullOrEmpty(document.Info.Contact.Name))
                {
                    metadata.Add(new Dictionary<string, string> { ["contact_name"] = document.Info.Contact.Name });
                }
            }

            return metadata;
        }

        /// <summary>
        /// Creates test cases from the OpenAPI document paths and operations.
        /// </summary>
        /// <param name="document">The OpenAPI document</param>
        /// <returns>List of test cases</returns>
        private static IEnumerable<Test> CreateTests(Microsoft.OpenApi.Models.OpenApiDocument document)
        {
            var tests = new List<Test>();

            if (document.Paths == null)
                return tests;

            foreach (var path in document.Paths)
            {
                var pathItem = path.Value;
                var pathUrl = path.Key;

                // Skip paths with path parameters for now (can be extended later)
                if (pathUrl.Contains('{'))
                    continue;

                foreach (var operation in pathItem.Operations)
                {
                    var test = CreateTestFromOperation(pathUrl, operation.Key, operation.Value);
                    if (test != null)
                    {
                        tests.Add(test);
                    }
                }
            }

            return tests;
        }

        /// <summary>
        /// Creates a test case from an OpenAPI operation.
        /// </summary>
        /// <param name="pathUrl">The path URL</param>
        /// <param name="method">The HTTP method</param>
        /// <param name="operation">The OpenAPI operation</param>
        /// <returns>A test case or null if the operation should be skipped</returns>
        private static Test CreateTestFromOperation(string pathUrl, Microsoft.OpenApi.Models.OperationType method, Microsoft.OpenApi.Models.OpenApiOperation operation)
        {
            var test = new Test
            {
                Name = $"{method.ToString().ToUpperInvariant()} {pathUrl}",
                Url = pathUrl,
                Method = ConvertHttpMethod(method),
                Tags = operation.Tags?.Select(t => t.Name).ToList() ?? [],
                Headers = CreateHeaders(operation),
                //Validators = CreateValidators(operation),
                Context = [],
                Metadata = CreateTestMetadata(operation)
            };

            // Handle query parameters
            if (operation.Parameters != null)
            {
                var queryParams = operation.Parameters
                    .Where(p => p.In == Microsoft.OpenApi.Models.ParameterLocation.Query)
                    .Select(p => $"{p.Name}={GenerateSampleValue(p.Schema)}")
                    .ToList();

                if (queryParams.Any())
                {
                    test.Url = $"{test.Url}?{string.Join("&", queryParams)}";
                }
            }

            // Handle request body
            if (operation.RequestBody?.Content != null)
            {
                var jsonContent = operation.RequestBody.Content
                    .FirstOrDefault(c => c.Key.Contains("json", StringComparison.OrdinalIgnoreCase));

                if (jsonContent.Value != null)
                {
                    test.Body = GenerateRequestBody(jsonContent.Value);
                }
            }

            return test;
        }

        /// <summary>
        /// Converts OpenAPI operation type to Testlemon HTTP method.
        /// </summary>
        /// <param name="method">OpenAPI operation type</param>
        /// <returns>Testlemon HTTP method</returns>
        private static HttpMethod ConvertHttpMethod(Microsoft.OpenApi.Models.OperationType method)
        {
            return method switch
            {
                Microsoft.OpenApi.Models.OperationType.Get => HttpMethod.Get,
                Microsoft.OpenApi.Models.OperationType.Post => HttpMethod.Post,
                Microsoft.OpenApi.Models.OperationType.Put => HttpMethod.Put,
                Microsoft.OpenApi.Models.OperationType.Delete => HttpMethod.Delete,
                Microsoft.OpenApi.Models.OperationType.Patch => HttpMethod.Patch,
                Microsoft.OpenApi.Models.OperationType.Head => HttpMethod.Head,
                Microsoft.OpenApi.Models.OperationType.Options => HttpMethod.Options,
                Microsoft.OpenApi.Models.OperationType.Trace => HttpMethod.Trace,
                _ => HttpMethod.Get
            };
        }

        /// <summary>
        /// Creates headers for the test case.
        /// </summary>
        /// <param name="operation">The OpenAPI operation</param>
        /// <returns>List of header strings</returns>
        private static List<string> CreateHeaders(Microsoft.OpenApi.Models.OpenApiOperation operation)
        {
            var headers = new List<string>
            {
                // Add common headers
                "Content-Type: application/json",
                "Accept: application/json"
            };

            // Add operation-specific headers
            if (operation.Parameters != null)
            {
                var headerParams = operation.Parameters
                    .Where(p => p.In == Microsoft.OpenApi.Models.ParameterLocation.Header)
                    .Select(p => $"{p.Name}: {GenerateSampleValue(p.Schema)}")
                    .ToList();

                headers.AddRange(headerParams);
            }

            return headers;
        }

        /// <summary>
        /// Creates validators for the test case based on responses.
        /// </summary>
        /// <param name="operation">The OpenAPI operation</param>
        /// <returns>List of validator dictionaries</returns>
        private static List<Dictionary<string, string>> CreateValidators(Microsoft.OpenApi.Models.OpenApiOperation operation)
        {
            var validators = new List<Dictionary<string, string>>();

            if (operation.Responses != null)
            {
                // Add status code validators for successful responses
                var successResponses = operation.Responses
                    .Where(r => r.Key.StartsWith("2"))
                    .ToList();

                if (successResponses.Any())
                {
                    validators.Add(new Dictionary<string, string>
                    {
                        ["StatusCode"] = successResponses.First().Key
                    });
                }
                else
                {
                    // Default to 200 if no success responses defined
                    validators.Add(new Dictionary<string, string>
                    {
                        ["StatusCode"] = "200"
                    });
                }
            }

            return validators;
        }

        /// <summary>
        /// Creates metadata for the test case.
        /// </summary>
        /// <param name="operation">The OpenAPI operation</param>
        /// <returns>List of metadata dictionaries</returns>
        private static List<Dictionary<string, string>> CreateTestMetadata(Microsoft.OpenApi.Models.OpenApiOperation operation)
        {
            var metadata = new List<Dictionary<string, string>>();

            if (!string.IsNullOrEmpty(operation.Summary))
            {
                metadata.Add(new Dictionary<string, string> { ["summary"] = operation.Summary });
            }

            if (!string.IsNullOrEmpty(operation.Description))
            {
                metadata.Add(new Dictionary<string, string> { ["description"] = operation.Description });
            }

            if (operation.OperationId != null)
            {
                metadata.Add(new Dictionary<string, string> { ["operationId"] = operation.OperationId });
            }

            return metadata;
        }

        /// <summary>
        /// Generates a sample value for a schema.
        /// </summary>
        /// <param name="schema">The OpenAPI schema</param>
        /// <returns>A sample value as string</returns>
        private static string GenerateSampleValue(Microsoft.OpenApi.Models.OpenApiSchema schema)
        {
            if (schema == null)
                return "sample";

            if (schema.Example != null)
            {
                return ConvertOpenApiAnyToString(schema.Example);
            }

            if (schema.Default != null)
            {
                return ConvertOpenApiAnyToString(schema.Default);
            }

            return schema.Type switch
            {
                "string" => schema.Format switch
                {
                    "uuid" => Guid.NewGuid().ToString(),
                    "date-time" => DateTime.UtcNow.ToString("o"),
                    "email" => "user@example.com",
                    "uri" => "https://example.com",
                    _ => "sample-string"
                },
                "integer" => "0",
                "number" => "0.0",
                "boolean" => "true",
                "array" => "[]",
                "object" => "{}",
                _ => "sample"
            };
        }

        /// <summary>
        /// Generates a request body from OpenAPI content.
        /// </summary>
        /// <param name="content">The OpenAPI content</param>
        /// <returns>JSON string representation of the request body</returns>
        private static string GenerateRequestBody(Microsoft.OpenApi.Models.OpenApiMediaType content)
        {
            if (content.Example != null)
            {
                var native = ConvertOpenApiAnyToNative(content.Example);
                return JsonSerializer.Serialize(native, JsonSerializerOptions);
            }

            if (content.Schema != null)
            {
                var sampleObject = GenerateSampleObject(content.Schema);
                return JsonSerializer.Serialize(sampleObject, JsonSerializerOptions);
            }

            return "{}";
        }

        /// <summary>
        /// Generates a sample object from an OpenAPI schema.
        /// </summary>
        /// <param name="schema">The OpenAPI schema</param>
        /// <returns>A sample object</returns>
        private static object GenerateSampleObject(Microsoft.OpenApi.Models.OpenApiSchema schema)
        {
            if (schema.Type == "array")
            {
                var items = schema.Items;
                if (items != null)
                {
                    return new[] { GenerateSampleObject(items) };
                }
                return new object[0];
            }

            if (schema.Type == "object" && schema.Properties != null)
            {
                var obj = new Dictionary<string, object>();
                foreach (var property in schema.Properties)
                {
                    obj[property.Key] = GenerateSampleObject(property.Value);
                }
                return obj;
            }

            return GenerateSampleValue(schema);
        }

        /// <summary>
        /// Converts an IOpenApiAny into a JSON-friendly native .NET object.
        /// </summary>
        private static object? ConvertOpenApiAnyToNative(IOpenApiAny any)
        {
            switch (any)
            {
                case null:
                    return null;
                case OpenApiString s:
                    return s.Value;
                case OpenApiInteger i:
                    return i.Value;
                case OpenApiLong l:
                    return l.Value;
                case OpenApiDouble d:
                    return d.Value;
                case OpenApiFloat f:
                    return f.Value;
                case OpenApiBoolean b:
                    return b.Value;
                case OpenApiDateTime dt:
                    return dt.Value;
                case OpenApiDate d:
                    return d.Value;
                case OpenApiByte b8:
                    return b8.Value;
                case OpenApiBinary bin:
                    return bin.Value;
                case OpenApiPassword p:
                    return p.Value;
                case OpenApiNull:
                    return null;
                case OpenApiArray arr:
                    return arr.Select(a => ConvertOpenApiAnyToNative(a)).ToArray();
                case OpenApiObject obj:
                    return obj.ToDictionary(kvp => kvp.Key, kvp => ConvertOpenApiAnyToNative(kvp.Value));
                default:
                    return any.ToString();
            }
        }

        /// <summary>
        /// Converts an IOpenApiAny into a string representation, preserving numeric precision (e.g., long).
        /// Arrays/objects are serialized to JSON strings.
        /// </summary>
        private static string ConvertOpenApiAnyToString(IOpenApiAny any)
        {
            switch (any)
            {
                case null:
                    return "sample";
                case OpenApiString s:
                    return s.Value ?? "";
                case OpenApiInteger i:
                    return i.Value.ToString(CultureInfo.InvariantCulture);
                case OpenApiLong l:
                    return l.Value.ToString(CultureInfo.InvariantCulture);
                case OpenApiDouble d:
                    return d.Value.ToString(CultureInfo.InvariantCulture);
                case OpenApiFloat f:
                    return f.Value.ToString(CultureInfo.InvariantCulture);
                case OpenApiBoolean b:
                    return b.Value ? "true" : "false";
                case OpenApiDateTime dt:
                    return dt.Value.ToString("o");
                case OpenApiDate d:
                    return d.Value.ToString("yyyy-MM-dd");
                case OpenApiByte b8:
                    return Convert.ToBase64String(b8.Value ?? Array.Empty<byte>());
                case OpenApiBinary bin:
                    return Convert.ToBase64String(bin.Value ?? Array.Empty<byte>());
                case OpenApiPassword p:
                    return p.Value ?? "";
                case OpenApiNull:
                    return "null";
                case OpenApiArray arr:
                    return JsonSerializer.Serialize(arr.Select(ConvertOpenApiAnyToNative).ToArray(), JsonSerializerOptions);
                case OpenApiObject obj:
                    return JsonSerializer.Serialize(obj.ToDictionary(kvp => kvp.Key, kvp => ConvertOpenApiAnyToNative(kvp.Value)), JsonSerializerOptions);
                default:
                    return any.ToString() ?? "sample";
            }
        }
    }
}
