namespace Testlemon.Core.CollectionProviders
{
    internal class DirectoryCollectionProvider(string path) : ICollectionProvider
    {
        private readonly string _path = path;

        public async Task<IEnumerable<string>> GetAsync()
        {
            if (!Directory.Exists(_path))
                throw new FileNotFoundException($"Directory with the following path does not exist: {_path}");

            var files = Directory.GetFiles(_path, "*.json", SearchOption.AllDirectories);
            var result = new List<string>();

            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file);
                result.Add(content);
            }

            return result;
        }
    }
}