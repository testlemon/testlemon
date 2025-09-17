using System.Text.RegularExpressions;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;
using Testlemon.LLMClient;

namespace Testlemon.Core.Validators
{
    public class PromptValidator(LlmClient llmClient) : LLMValidator(llmClient), IHttpResponseValidator
    {
        public string Name => "prompt";
        const string MODEL_PROMPT_REGEX_PATTERN = "^(.*?):(.*)$";
        const string PROMPT_TEMPLATE = @"
        Act as an experiences software tester and business analyst.
        You are given the following HTTP response from the web endpoint: '{0}'.
        Response can be in JSON format or in a plan text.
        Check if the following condition is true: '{1}'.
        Return 'True' if the condition holds in the response or 'False' if it does not.
        Respond only with 'True' or 'False'.";

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var body = response?.Body;
            var value = validator.Value;

            var match = Regex.Match(value, MODEL_PROMPT_REGEX_PATTERN);
            if (!match.Success)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Expected value invalid format. Format: {MODEL_PROMPT_REGEX_PATTERN}, but was {value}"
                };
            }

            var model = match.Groups[1].Value.Trim();
            var prompt = match.Groups[2].Value.Trim();

            var input = string.Format(PROMPT_TEMPLATE, body, prompt);

            var completion = await LLMClient.GetCompletionAsync(model, input);
            if (!bool.TryParse(completion, out bool result))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"The LLM completion niether '{true}', nor '{false}'",
                    Value = completion
                };
            }

            return new ValidationResult
            {
                IsSuccessful = result,
                Value = completion
            };
        }
    }
}