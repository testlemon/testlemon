using Testlemon.Core.CollectionProviders;
using Testlemon.Core.Models;

namespace Testlemon.Core
{
    public class CollectionRunner(
        DataProcessor dataProcessor,
        ValidationProcessor validationProcessor) : InternalCollectionRunner(dataProcessor, validationProcessor)
    {
        public async Task<IEnumerable<CollectionResult>> ExecuteAsync(
            IEnumerable<string> sources,
            Dictionary<string, string> headers,
            Dictionary<string, string>? variables = null,
            Dictionary<string, string>? secrets = null,
            IEnumerable<string>? tags = null,
            bool parallel = true,
            uint repeats = 1,
            uint delay = 0,
            bool followRedirect = true)
        {
            if (repeats == 0)
                return [];

            var collections = await GetCollectionsAsync(sources, headers);
            if (collections == null || !collections.Any())
            {
                throw new ArgumentException("No collections found. Please provide valid collection sources.", nameof(sources));
            }

            // Log the license information
            Console.WriteLine($"Testlemon is running.");

            var collectionResults = await ExecuteAsync(collections, variables, secrets, tags, parallel, repeats, delay, followRedirect);
            return collectionResults;
        }

        private static async Task<IEnumerable<Collection>> GetCollectionsAsync(IEnumerable<string> collections, Dictionary<string, string> headers)
        {
            var result = new List<Collection>();

            foreach (var collectionSource in collections)
            {
                // get relevant collection provider
                var provider = CollectionProviderFactory.GetProvider(collectionSource, headers);

                // get collection data
                var collectionsData = await provider.GetAsync();

                // iterate over the collection data and parse the content
                foreach (var collectionData in collectionsData)
                {
                    // try to parse the collection, OpenApi, Yaml or Json
                    if (!CollectionParser.TryParseOpenApi(collectionSource, collectionData, out Collection? collection))
                    {
                        if (!CollectionParser.TryParseJson(collectionData, out collection))
                        {
                            if (!CollectionParser.TryParseYaml(collectionData, out collection))
                            {
                                throw new ArgumentException($"Collection format is not correct. Source: {collectionData}");
                            }
                        }
                    }

                    if (collection != null)
                    {
                        result.Add(collection);
                    }
                }
            }

            return result;
        }
    }
}