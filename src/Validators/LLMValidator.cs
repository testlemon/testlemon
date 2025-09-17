using Testlemon.LLMClient;

namespace Testlemon.Core.Validators
{
    public abstract class LLMValidator(LlmClient llmClient)
    {
        protected readonly LlmClient LLMClient = llmClient;
    }
}