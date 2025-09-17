using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;
using Testlemon.LLMClient;

namespace Testlemon.Core.Validators
{
    public class SentimentValidator(LlmClient llmClient) : LLMValidator(llmClient), IHttpResponseValidator
    {
        public string Name => "sentiment";

        const string PROMPT_TEMPLATE = "Classify the text into neutral, negative, or positive. There should be only one word in the response. Text: '{0}'. Sentiment:";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var body = response?.Body;
            var sentiment = validator.Value;

            var prompt = string.Format(PROMPT_TEMPLATE, body);

            var completion = await LLMClient.GetCompletionAsync("gpt-4o", prompt);

            return new ValidationResult
            {
                IsSuccessful = completion.Contains(sentiment, StringComparison.OrdinalIgnoreCase),
                Value = $"{completion}"
            };
        }
    }
}