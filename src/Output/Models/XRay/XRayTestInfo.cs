namespace Testlemon.Core.Output.Models.XRay
{
    public class XRayTestInfo
    {
        public string? ProjectKey { get; set; }
        public string? Type { get; set; }
        public string? Summary { get; set; }
        public string? Definition { get; set; }
        public List<string>? RequirementKeys { get; set; }
    }
}