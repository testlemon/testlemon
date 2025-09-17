using System.Text;
using Testlemon.Core.Models.DFS;

namespace Testlemon.Core.Models
{
    public class Test : INode
    {
        public Test()
        {
            Method = HttpMethod.Get; // by default http method is set to GET
            Headers = [];
            Validators = [];
            Tags = [];
            Context = [];
        }

        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? DependsOn { get; set; }

        public required string Url { get; set; }

        public HttpMethod Method { get; set; }

        public string? Body { get; set; }

        public long BodySize => Encoding.UTF8.GetBytes(Body ?? string.Empty).Length;

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<string> Headers { get; set; }

        private string HeadersString => string.Join("", Headers) ?? string.Empty;

        public long HeadersSize => Encoding.UTF8.GetBytes(HeadersString).Length;

        public IEnumerable<Dictionary<string, string>> Validators { get; set; }

        public IEnumerable<Context> Context { get; set; }

        public IEnumerable<Dictionary<string, string>> Metadata { get; set; }

        public IEnumerable<Header> GetParsedHeaders()
        {
            return Headers.Select(x =>
            {
                var parts = x.Split(':');
                return new Header
                {
                    Key = parts[0].Trim(),
                    Values = parts.Skip(1).Select(x => x.Trim())
                };
            });
        }

        public IEnumerable<Validator> GetParsedValidators()
        {
            return Validators.Select(x =>
            {
                var element = x.FirstOrDefault();

                return new Validator
                {
                    Name = element.Key,
                    Value = element.Value.Trim()
                };
            });
        }
    }
}