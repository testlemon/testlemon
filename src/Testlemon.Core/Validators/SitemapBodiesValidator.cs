using Testlemon.Core.Models;

namespace Testlemon.Core.Validators
{
    public abstract class SitemapBodiesValidator : SitemapValidator
    {
        protected const int SNIPPET_LENGTH = 200;

        private readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromMinutes(5) // set default timeout to 5 minutes TODO: make configurable
        };

        protected abstract IValidationResult CompareBodies(Dictionary<Uri, string> bodies, string keyword);

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var entries = await GetSitemapEntries(test.Url);

            var locations = entries
                .SelectMany(x => x.Urls)
                .DistinctBy(x => x.Location)
                .Select(x => x.Location);

            if (!locations.Any())
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Could not find any URLs."
                };
            }

            var keyword = validator.Value;

            var bodies = new Dictionary<Uri, string>();
            foreach (var location in locations)
            {
                var content = await _httpClient.GetStringAsync(location);
                bodies.Add(location, content);
            }

            return CompareBodies(bodies, keyword);
        }

        protected static string? GetSubstringWithContext(string input, string substring, int contextLength)
        {
            int index = input.IndexOf(substring);

            if (index == -1)
            {
                return null; // Substring not found
            }

            // Calculate start and end positions considering boundaries
            int start = Math.Max(0, index - contextLength);
            int end = Math.Min(input.Length, index + substring.Length + contextLength);

            // Extract and return the substring with context
            return input[start..end];
        }
    }
}