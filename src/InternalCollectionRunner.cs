using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Testlemon.Core.DFS;
using Testlemon.Core.Helpers;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Testlemon.Core;

public class InternalCollectionRunner(
    DataProcessor dataProcessor,
    ValidationProcessor validationProcessor)
{
    private readonly DataProcessor _dataProcessor = dataProcessor;
    private readonly ValidationProcessor _validationProcessor = validationProcessor;
    private readonly NodeProcessor<Test, TestResult> _nodeProcessor = new();
    private readonly ConcurrentDictionary<string, string> _variablesContext = new();
    private readonly ConcurrentDictionary<string, string> _secretsContext = new();
    private readonly ConcurrentDictionary<string, string> _variables = new();
    private readonly ConcurrentDictionary<string, string> _secrets = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    protected async Task<IEnumerable<CollectionResult>> ExecuteAsync(
        IEnumerable<Collection> collections,
        Dictionary<string, string>? variables,
        Dictionary<string, string>? secrets,
        IEnumerable<string>? tags,
        bool parallel,
        uint repeats,
        uint delay,
        bool followRedirect = true)
    {
        // Run the collection(s) of tests
        var httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = followRedirect
        })
        {
            Timeout = TimeSpan.FromMinutes(5) // set default timeout for http client to 5 minutes
        };

        var tasks = collections.Select(collection => ExecuteAsync(httpClient, collection, variables, secrets, tags, parallel, repeats, delay)).ToArray();

        await Task.WhenAll(tasks);

        // get all results
        var collectionResults = tasks.Select(task => task.Result);
        return collectionResults;
    }

    private async Task<CollectionResult> ExecuteAsync(
        HttpClient httpClient,
        Collection collection,
        Dictionary<string, string>? variables,
        Dictionary<string, string>? secrets,
        IEnumerable<string>? tags,
        bool parallel,
        uint repeats,
        uint delay)
    {
        Console.WriteLine($"Running the collection '{collection.Name}' {repeats} time(s).");

        // filter tests by tags if they exist.
        var tests = tags != null && tags.Any()
                                ? collection.Tests.Where(test => test.Tags.Intersect(tags).Any())
                                : collection.Tests;

        if (!tests.Any())
            return new CollectionResult()
            {
                Name = collection.Name,
                TestsResults = []
            };

        foreach (var variable in variables ?? [])
        {
            _variables[variable.Key] = variable.Value;
        }

        foreach (var secret in secrets ?? [])
        {
            _secrets[secret.Key] = secret.Value;
        }

        foreach (var test in tests)
        {
            if (test.Url.StartsWith('/'))
                test.Url = $"{collection.BaseUrl}{test.Url}";
        }

        var tasks = new List<Task<TestResult?>>();
        var stopWatch = Stopwatch.StartNew();

        for (var i = 0; i < repeats; i++)
        {
            var task = _nodeProcessor.ProcessNodes(tests, test => SafeExecuteRequestAsync(httpClient, test), parallel);
            tasks.AddRange(task);

            if (delay > 0)
            {
                Console.WriteLine($"Delay {delay} ms.");
                await Task.Delay((int)delay);
            }
        }

        await Task.WhenAll(tasks);
        stopWatch.Stop();

        var testResults = tasks.Select(x => x.Result);
        var collectionResult = new CollectionResult
        {
            Name = collection.Name,
            TestsResults = testResults,
            Metadata = collection.Metadata,
            AverageTtfb = TimeSpan.FromMilliseconds(testResults.Average(x => x?.Response?.Ttfb.TotalMilliseconds ?? 0)),
            AverageContentDownloadTime = TimeSpan.FromMilliseconds(testResults.Average(x => x?.Response?.ContentDownloadTime.TotalMilliseconds ?? 0)),
            AverageTotalDuration = TimeSpan.FromMilliseconds(testResults.Average(x => x?.Response?.TotalDuration.TotalMilliseconds ?? 0)),
            TotalDuration = stopWatch.Elapsed
        };

        return collectionResult;
    }

    private async Task<TestResult?> SafeExecuteRequestAsync(HttpClient client, Test testTemplate)
    {
        // enrich request with variables, secrets, functions and context values.
        var test = ReplaceRequestTokens(testTemplate);
        Response? response;

        // if no validators, add successful response validator by default
        if (!test.Validators.Any())
        {
            test.Validators = [
                new Dictionary<string, string>()
                    {
                        { "is-successful", true.ToString() }
                    }
            ];
        }

        var requestValidators = test.GetParsedValidators();
        var validators = requestValidators.Select(x => _validationProcessor.GetValidator(x));

        try
        {
            // make a http requests only in case there are validators which require that.
            if (validators.Any(x => x is IHttpResponseValidator))
            {
                response = await ExecuteRequestAsync(client, test);
            }
            else
            {
                response = new Response
                {
                    IsSuccessStatusCode = false,
                    StatusCode = -1,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing test name: '{test.Name}', url: '{test.Url}'. Error: {ex.Message}");
            response = new Response
            {
                IsSuccessStatusCode = false,
                StatusCode = -1,
            };
        }

        var validationResults = requestValidators
            .Select(async x => await ValidateAsync(test, response, x))
            .Select(x => x.Result)
            .ToList();

        var requestResult = new TestResult
        {
            Test = test,
            Response = response,
            Validators = validationResults
        };

        return requestResult;
    }

    private async Task<Response?> ExecuteRequestAsync(HttpClient client, Test test)
    {
        var message = new HttpRequestMessage(HttpMethod.Parse(test.Method.ToString()), test.Url)
        {
            Content = test.Body != null ? new StringContent(test.Body, Encoding.UTF8, "application/json") : null
        };

        // add all headers from the request to the request message
        foreach (var header in test.GetParsedHeaders())
        {
            if (HttpHeadersHelper.IsRequestHeader(header.Key))
            {
                message.Headers.Remove(header.Key);
                message.Headers.Add(header.Key, header.Values);
            }

            if (HttpHeadersHelper.IsContentHeader(header.Key))
            {
                message.Content?.Headers.Remove(header.Key);
                message.Content?.Headers.Add(header.Key, header.Values);
            }
        }

        var response = await ExecuteAndCalculateTiming(client, message);

        // save response to context
        foreach (var context in test.Context)
        {
            var name = context.Name;
            var match = Regex.Match(response?.Body ?? string.Empty, context.Pattern);
            var value = match.Groups.Values.Last().Value;

            if (context.Type == ContextType.Variable)
            {
                _variablesContext[name] = value;
            }
            else
            {
                _secretsContext[name] = value;
            }
        }

        return response;
    }

    private static async Task<Response?> ExecuteAndCalculateTiming(HttpClient client, HttpRequestMessage message)
    {
        if (message == null || message.RequestUri == null)
            return null;

        // calculate ttfb and total time
        var stopWatch = Stopwatch.StartNew();
        var requestDate = DateTime.UtcNow;

        //Console.WriteLine(JsonSerializer.Serialize(message));
        var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);

        var ttfb = stopWatch.Elapsed;
        stopWatch.Restart();

        var body = await response.Content.ReadAsStringAsync();
        var contentDownloadTime = stopWatch.Elapsed;
        stopWatch.Stop();

        // log request summary with 8 characters reserved for HTTP method
        Console.WriteLine($"{message.Method,-8} {message.RequestUri} => {response.StatusCode} in {ttfb + stopWatch.Elapsed}");

        var result = new Response
        {
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode, // 200, 404, etc.
            Ttfb = ttfb,
            ContentDownloadTime = contentDownloadTime,
            CreatedDate = requestDate + ttfb + contentDownloadTime,
            Body = body,
            Headers = response.Headers.Select(x => new Header()
            {
                Key = x.Key,
                Values = x.Value
            })
        };

        return result;
    }

    private Test ReplaceRequestTokens(Test test)
    {
        var json = JsonSerializer.Serialize(test, _jsonSerializerOptions);

        json = _dataProcessor.SubstituteVariables(json, _variables.ToDictionary());
        json = _dataProcessor.SubstituteSecrets(json, _secrets.ToDictionary());
        json = _dataProcessor.SubstituteFunctions(json);
        json = _dataProcessor.SubstituteContexts(json, _variablesContext.ToDictionary());
        json = _dataProcessor.SubstituteContexts(json, _secretsContext.ToDictionary());
        json = _dataProcessor.SubstituteOpenAI(json);

        var request = JsonSerializer.Deserialize<Test>(json, _jsonSerializerOptions) ?? throw new Exception("Error during request data processing.");
        return request;
    }

    private TestResult MaskSecrets(TestResult testResult)
    {
        var json = JsonSerializer.Serialize(testResult, _jsonSerializerOptions);

        json = _dataProcessor.MaskSecrets(json, _secrets.ToDictionary());
        json = _dataProcessor.MaskSecrets(json, _secretsContext.ToDictionary());

        var request = JsonSerializer.Deserialize<TestResult>(json, _jsonSerializerOptions) ?? throw new Exception("Error during request data processing.");
        return request;
    }

    private async Task<ValidatorResult> ValidateAsync(Test test, Response? response, Validator val)
    {
        var result = await _validationProcessor.ValidateAsync(test, response, val);

        return new ValidatorResult
        {
            Validator = val,
            Result = result
        };
    }
}
