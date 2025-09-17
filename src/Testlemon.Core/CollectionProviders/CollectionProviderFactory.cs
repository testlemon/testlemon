namespace Testlemon.Core.CollectionProviders
{
    internal class CollectionProviderFactory
    {
        public static ICollectionProvider GetProvider(string collection, Dictionary<string, string> headers)
        {
            if (Uri.IsWellFormedUriString(collection, UriKind.Absolute))
                return new UrlCollectionProvider(collection, headers);

            if (Directory.Exists(collection))
                return new DirectoryCollectionProvider(collection);

            if (File.Exists(collection))
                return new FileCollectionProvider(collection);

            if (!string.IsNullOrWhiteSpace(collection))
                return new InlineCollectionProvider(collection); //TODO: maybe it make sence to try to parse the collection before returning the inline

            throw new NotImplementedException("Unable to parse the source for collection.");
        }
    }
}