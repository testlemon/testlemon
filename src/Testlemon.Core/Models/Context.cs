namespace Testlemon.Core.Models
{
    public class Context
    {
        public required string Name { get; set; }

        public required ContextType Type { get; set; }

        public required string Pattern { get; set; }
    }
}