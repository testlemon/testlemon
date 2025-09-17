using System.Text.Json;
using System.Text.RegularExpressions;
using Testlemon.LLMClient;

namespace Testlemon.Core
{
    public class DataProcessor(LlmClient lmmClient)
    {
        private readonly LlmClient _lmmClient = lmmClient;

        public string SubstituteVariables(string inputTemplate, Dictionary<string, string> variables)
        {
            var tokens = variables.ToDictionary(x => $"${{{{ vars.{x.Key} }}}}", x => x.Value);
            return MultipleReplace(inputTemplate, tokens);
        }

        public string SubstituteSecrets(string inputTemplate, Dictionary<string, string> secrets)
        {
            var tokens = secrets.ToDictionary(x => $"${{{{ secrets.{x.Key} }}}}", x => x.Value);
            return MultipleReplace(inputTemplate, tokens);
        }

        public string SubstituteContexts(string inputTemplate, Dictionary<string, string> contexts)
        {
            var tokens = contexts.ToDictionary(x => $"${{{{ context.{x.Key} }}}}", x => x.Value);
            return MultipleReplace(inputTemplate, tokens);
        }

        public string SubstituteFunctions(string inputTemplate)
        {
            var tokens = new Dictionary<string, string>
            {
                { "${{ func.utcnow() }}", DateTime.UtcNow.ToString("o") },
                { "${{ func.random() }}", new Random().Next().ToString() },
                { "${{ func.guid() }}", Guid.NewGuid().ToString() },
            };

            return MultipleReplace(inputTemplate, tokens);
        }

        public string MaskSecrets(string output, Dictionary<string, string> secrets)
        {
            secrets = secrets.ToDictionary(x => x.Value, x => "*****");

            return MultipleReplace(output, secrets);
        }

        public string SubstituteOpenAI(string inputTemplate)
        {
            var aiCommandPattern = @"\$\{\{\s*([\w\-]+)\.(\w+)\((\d+)\)\s*\}\}";
            var matches = Regex.Matches(inputTemplate, aiCommandPattern);

            var tokens = matches
                    .DistinctBy(match => match.Value)
                    .ToDictionary(
                        match => match.Value,
                        match =>
                        {
                            // Extract the groups
                            var model = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
                            var action = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
                            var value = match.Groups[3].Success ? match.Groups[3].Value : string.Empty;

                            return new Func<Task<string>>(() => GetLlmResponse(model, action, value));
                        }
                    );

            return MultipleReplace(inputTemplate, tokens);
        }

        private async Task<string> GetLlmResponse(string model, string action, string value)
        {
            var promptTemplate = "Meaningful human readable text exactly {0} characters long";
            var adjustedValue = string.IsNullOrWhiteSpace(value) ? "100" : value;
            var prompt = string.Format(promptTemplate, adjustedValue);

            return action switch
            {
                "text" => await _lmmClient.GetCompletionAsync(model, prompt),
                _ => throw new NotImplementedException("LLM action is not implemented."),
            };
        }

        private string MultipleReplace(string input, Dictionary<string, string> tokens)
        {
            // Create a regex pattern that matches any of the keys in the dictionary
            var pattern = string.Join("|", tokens.Keys.Select(Regex.Escape));

            if (string.IsNullOrWhiteSpace(pattern))
                return input;

            // Use Regex.Replace with a MatchEvaluator delegate to perform the replacements
            return Regex.Replace(input, pattern, match => EscapeStringForJson(tokens[match.Value]));
        }

        private string MultipleReplace(string input, Dictionary<string, Func<Task<string>>> tokens)
        {
            // Create a regex pattern that matches any of the keys in the dictionary
            var pattern = string.Join("|", tokens.Keys.Select(Regex.Escape));

            if (string.IsNullOrWhiteSpace(pattern))
                return input;

            // Use Regex.Replace with a MatchEvaluator delegate to perform the replacements
            return Regex.Replace(input, pattern, match =>
            {
                var value = tokens[match.Value].Invoke().Result;
                return EscapeStringForJson(value);
            });
        }

        private static string EscapeStringForJson(string input)
        {
            var jsonString = JsonSerializer.Serialize(input).Trim('"');
            jsonString = JsonSerializer.Serialize(jsonString).Trim('"');
            return jsonString;
        }
    }
}