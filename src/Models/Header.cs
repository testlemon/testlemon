namespace Testlemon.Core.Models
{
    public class Header
    {
        public required string Key { get; set; }

        public required IEnumerable<string> Values { get; set; }
    }
}