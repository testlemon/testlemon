using Testlemon.Core.Models;

namespace Testlemon.Core.Output
{
    public static class CollectionResultOutputExtensions
    {
        public static string ToXRayOutput(this CollectionResult result)
        {
            return XRayOutputFormatter.Format(result);
        }

        public static string ToJsonOutput(this CollectionResult result)
        {
            return JsonOutputFormatter.Format(result);
        }
    }
}