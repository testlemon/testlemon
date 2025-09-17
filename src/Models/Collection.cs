namespace Testlemon.Core.Models
{
    public class Collection
    {
        public Collection()
        {
            Tests = [];
            Name = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }

        public string? BaseUrl { get; set; }

        public IEnumerable<Dictionary<string, string>> Metadata { get; set; }

        public IEnumerable<Test> Tests { get; set; }
    }
}