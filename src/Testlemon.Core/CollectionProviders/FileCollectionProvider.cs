namespace Testlemon.Core.CollectionProviders
{
    internal class FileCollectionProvider(string path) : ICollectionProvider
    {
        private readonly string _path = path;

        public async Task<IEnumerable<string>> GetAsync()
        {
            if (!File.Exists(_path))
                throw new FileNotFoundException($"File with the following path does not exist: {_path}");

            var fileContent = await File.ReadAllTextAsync(_path);

            return [fileContent];
        }
    }
}