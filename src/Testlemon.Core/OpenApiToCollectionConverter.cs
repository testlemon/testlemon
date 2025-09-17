using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Testlemon.Core.Models;
using HttpMethod = Testlemon.Core.Models.HttpMethod;

namespace Testlemon.Core
{
    public class OpenApiToCollectionConverter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static Collection ConvertOpenApiToCollection(string openApiJson, string source = "")
        {
            using var document = JsonDocument.Parse(openApiJson);
            var root = document.RootElement;

            if (!root.TryGetProperty("servers", out var servers) || servers.GetArrayLength() == 0)
                throw new ArgumentException("OpenAPI spec must contain at least one server.");

            var schemas = root.TryGetProperty("components", out var components) && components.TryGetProperty("schemas", out var schemasProp)
                ? schemasProp
                : throw new ArgumentException("OpenAPI spec must contain components.schemas.");

            var baseUrl = servers[0].GetProperty("url").GetString()!.TrimEnd('/');
            if (Uri.IsWellFormedUriString(baseUrl, UriKind.Relative))
                baseUrl = $"{new Uri(source).Scheme}://{new Uri(source).Host}{baseUrl}";

            var collection = new Collection
            {
                BaseUrl = baseUrl,
                Metadata = []
            };

            var tests = new List<Test>();
            if (root.TryGetProperty("paths", out var paths))
            {
                foreach (var path in paths.EnumerateObject())
                {
                    string pathUrl = path.Name;
                    // Skip paths containing {id} or similar path parameters
                    if (pathUrl.Contains('{'))
                        continue;

                    foreach (var operation in path.Value.EnumerateObject())
                    {
                        var test = CreateTest(pathUrl, operation, schemas);
                        tests.Add(test);
                    }
                }
            }

            collection.Tests = tests;
            return collection;
        }

        private static Test CreateTest(string pathUrl, JsonProperty operation, JsonElement schemas)
        {
            var test = new Test
            {
                Name = $"{operation.Name.ToUpperInvariant()} {pathUrl}",
                Url = pathUrl,
                Method = ParseHttpMethod(operation.Name),
                Tags = operation.Value.TryGetProperty("tags", out var tags)
                    ? tags.EnumerateArray().Select(t => t.GetString()!).ToList()
                    : [],
                Headers = [],
                Validators = [],
                Context = [],
                Metadata = []
            };

            // Handle query parameters for GET
            if (operation.Name.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                operation.Value.TryGetProperty("parameters", out var parameters))
            {
                var queryParams = new List<string>();
                foreach (var param in parameters.EnumerateArray())
                {
                    if (param.TryGetProperty("in", out var inElem) && inElem.GetString() == "query")
                    {
                        var paramName = param.GetProperty("name").GetString()!;
                        var paramValue = GenerateSampleValue(param, schemas);
                        queryParams.Add($"{paramName}={Uri.EscapeDataString(paramValue)}");
                    }
                }
                if (queryParams.Any())
                {
                    test.Url = $"{test.Url}?{string.Join("&", queryParams)}";
                }
            }

            // Handle requestBody for POST, PUT, DELETE
            if (operation.Value.TryGetProperty("requestBody", out var requestBody) &&
                requestBody.TryGetProperty("content", out var content) &&
                content.TryGetProperty("application/json", out var jsonContent))
            {
                if (jsonContent.TryGetProperty("example", out var contentExample))
                {
                    test.Body = JsonSerializer.Serialize(contentExample, SerializerOptions);
                }
                else if (jsonContent.TryGetProperty("schema", out var schema))
                {
                    bool isArray = schema.TryGetProperty("type", out var schemaType) && schemaType.GetString() == "array";
                    string? schemaName = null;

                    if (isArray && schema.TryGetProperty("items", out var items) && items.TryGetProperty("$ref", out var itemsRef))
                    {
                        schemaName = itemsRef.GetString()?.Split('/').Last();
                    }
                    else if (schema.TryGetProperty("$ref", out var schemaRef))
                    {
                        schemaName = schemaRef.GetString()?.Split('/').Last();
                    }

                    if (schemaName != null)
                    {
                        var bodyObject = CreateDynamicObjectFromSchema(schemaName, schemas);
                        test.Body = JsonSerializer.Serialize(isArray ? new List<object> { bodyObject } : bodyObject, SerializerOptions);
                    }
                }
            }

            return test;
        }

        private static HttpMethod ParseHttpMethod(string method)
        {
            return method.ToLowerInvariant() switch
            {
                "get" => HttpMethod.Get,
                "post" => HttpMethod.Post,
                "put" => HttpMethod.Put,
                "delete" => HttpMethod.Delete,
                _ => HttpMethod.Get
            };
        }

        private static string GenerateSampleValue(JsonElement param, JsonElement schemas)
        {
            // Check for default value in schema
            if (param.TryGetProperty("schema", out var schema) && schema.TryGetProperty("default", out var defaultElem))
            {
                return defaultElem.ValueKind switch
                {
                    JsonValueKind.String => defaultElem.GetString()!,
                    JsonValueKind.Number => defaultElem.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => "sample"
                };
            }

            // Fall back to example value
            if (param.TryGetProperty("example", out var exampleElem))
            {
                return exampleElem.ValueKind switch
                {
                    JsonValueKind.String => exampleElem.GetString()!,
                    JsonValueKind.Number => exampleElem.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => "sample"
                };
            }

            // Fall back to schema-based generation
            if (!param.TryGetProperty("schema", out schema))
                return "sample";

            if (schema.TryGetProperty("type", out var typeElem))
            {
                string type = typeElem.GetString()!;
                switch (type)
                {
                    case "string":
                        if (schema.TryGetProperty("format", out var formatElem))
                        {
                            string format = formatElem.GetString()!;
                            return format switch
                            {
                                "uuid" => Guid.NewGuid().ToString(),
                                "date-time" => DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                                _ => "sample-string"
                            };
                        }
                        return "sample-string";
                    case "integer":
                        return "0";
                    case "number":
                        return "0.0";
                    case "boolean":
                        return "true";
                    default:
                        return "sample";
                }
            }

            return "sample";
        }

        private static object CreateDynamicObjectFromSchema(string schemaName, JsonElement schemas)
        {
            if (!schemas.TryGetProperty(schemaName, out var schemaElement))
                throw new ArgumentException($"Schema '{schemaName}' not found.");

            if (schemaElement.TryGetProperty("example", out var schemaExample))
            {
                return JsonSerializer.Deserialize<object>(schemaExample.GetRawText(), SerializerOptions)!;
            }

            var obj = new Dictionary<string, object>();

            if (schemaElement.TryGetProperty("properties", out var properties))
            {
                foreach (var prop in properties.EnumerateObject())
                {
                    obj[prop.Name] = GeneratePropertyValue(prop, schemaName, schemas);
                }

                // Handle required fields for User schema
                if (schemaName == "User" && schemaElement.TryGetProperty("required", out var required))
                {
                    foreach (var reqProp in required.EnumerateArray())
                    {
                        string propName = reqProp.GetString()!;
                        if (!obj.ContainsKey(propName) || obj[propName] == null)
                        {
                            obj[propName] = "sample-string";
                        }
                    }
                }
            }

            return obj;
        }

        private static object? GeneratePropertyValue(JsonProperty prop, string schemaName, JsonElement schemas)
        {
            var propSchema = prop.Value;

            // Check for default value
            if (propSchema.TryGetProperty("default", out var defaultElem))
            {
                return defaultElem.ValueKind switch
                {
                    JsonValueKind.String => defaultElem.GetString(),
                    JsonValueKind.Number => defaultElem.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Object => JsonSerializer.Deserialize<object>(defaultElem.GetRawText(), SerializerOptions),
                    JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(defaultElem.GetRawText(), SerializerOptions),
                    _ => null
                };
            }

            // Fall back to example value
            if (propSchema.TryGetProperty("example", out var propExample))
            {
                return propExample.ValueKind switch
                {
                    JsonValueKind.String => propExample.GetString(),
                    JsonValueKind.Number => propExample.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Object => JsonSerializer.Deserialize<object>(propExample.GetRawText(), SerializerOptions),
                    JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(propExample.GetRawText(), SerializerOptions),
                    _ => null
                };
            }

            // Fall back to schema-based generation
            if (propSchema.TryGetProperty("$ref", out var propRef))
            {
                string? refSchemaName = propRef.GetString()?.Split('/').Last();
                return refSchemaName != null ? CreateDynamicObjectFromSchema(refSchemaName, schemas) : null;
            }

            if (propSchema.TryGetProperty("type", out var typeElem))
            {
                string propType = typeElem.GetString()!;
                switch (propType)
                {
                    case "string":
                        if (propSchema.TryGetProperty("format", out var formatElem))
                        {
                            string format = formatElem.GetString()!;
                            return format switch
                            {
                                "uuid" => Guid.NewGuid().ToString(),
                                "date-time" => DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                                _ => schemaName == "User" && (prop.Name == "firstName" || prop.Name == "lastName" || prop.Name == "username" || prop.Name == "email")
                                    ? "sample-string"
                                    : null
                            };
                        }
                        return null;
                    case "number":
                        return 0.0;
                    case "integer":
                        return 0;
                    case "array":
                        var list = new List<object>();
                        if (propSchema.TryGetProperty("items", out var itemsElem) && itemsElem.TryGetProperty("$ref", out var itemsRef))
                        {
                            string? refSchemaName = itemsRef.GetString()?.Split('/').Last();
                            if (refSchemaName != null)
                            {
                                list.Add(CreateDynamicObjectFromSchema(refSchemaName, schemas));
                            }
                        }
                        return list;
                    default:
                        return null;
                }
            }

            return null;
        }
    }
}