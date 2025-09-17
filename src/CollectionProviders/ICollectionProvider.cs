namespace Testlemon.Core.CollectionProviders
{
    internal interface ICollectionProvider
    {
        Task<IEnumerable<string>> GetAsync();
    }
}