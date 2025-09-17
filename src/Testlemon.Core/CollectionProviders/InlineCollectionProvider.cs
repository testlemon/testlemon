namespace Testlemon.Core.CollectionProviders
{
    internal class InlineCollectionProvider(string inlineText) : ICollectionProvider
    {
        public readonly string _inlineText = inlineText;
        public async Task<IEnumerable<string>> GetAsync()
        {
            return await Task.FromResult(new string[] { _inlineText });
        }
    }
}