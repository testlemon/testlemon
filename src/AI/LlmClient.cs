namespace Testlemon.LLMClient
{
    public class LlmClient(OpenAIClient.OpenAIClient openAIClient, OllamaClient.OllamaClient ollamaClient)
    {
        private readonly OpenAIClient.OpenAIClient _openAIClient = openAIClient;
        private readonly OllamaClient.OllamaClient _ollamaClient = ollamaClient;

        public async Task<string> GetCompletionAsync(string model, string prompt)
        {
            string result = model switch
            {
                string s when s.Contains("gpt") => await _openAIClient.GetCompletionAsync(model, prompt),
                string s when s.Contains("gemma") => await _ollamaClient.GetCompletionAsync(model, prompt),
                _ => throw new Exception("Model is not supported."),
            };
            return result;
        }
    }
}