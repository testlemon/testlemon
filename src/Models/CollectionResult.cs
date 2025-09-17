namespace Testlemon.Core.Models
{
    public class CollectionResult
    {
        public CollectionResult()
        {
            TestsResults = [];
        }

        public required string Name { get; set; }

        public bool IsValid => TestsResults.All(x => x != null && x.IsValid);

        public required IEnumerable<TestResult?> TestsResults { get; set; }

        public IEnumerable<Dictionary<string, string>> Metadata { get; set; }

        public TimeSpan AverageTtfb { get; set; } // average from requests

        public TimeSpan AverageContentDownloadTime { get; set; } // average from requests

        public TimeSpan AverageTotalDuration { get; set; } // average from requests

        public TimeSpan TotalDuration { get; set; } // average from requests
    }
}