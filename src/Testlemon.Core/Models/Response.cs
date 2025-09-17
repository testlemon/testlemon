using System.Text;

namespace Testlemon.Core.Models
{
    public class Response
    {
        public bool IsSuccessStatusCode { get; set; }

        public int StatusCode { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public TimeSpan Ttfb { get; set; } // ms, time to first byte

        public TimeSpan ContentDownloadTime { get; set; }

        public TimeSpan TotalDuration => ContentDownloadTime + Ttfb;

        public IEnumerable<Header>? Headers { get; set; }

        private string HeadersString => string.Join("", Headers?.Select(header => header.Key + string.Join("", header.Values)) ?? [string.Empty]);

        public long HeadersSize => Encoding.UTF8.GetBytes(HeadersString).Length;

        public string? Body { get; set; }

        public long BodySize => Encoding.UTF8.GetBytes(Body ?? string.Empty).Length;
    }
}