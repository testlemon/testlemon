namespace Testlemon.Core.Output.Models.XRay
{
    public class XRayInfo
    {
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? Revision { get; set; }
        public List<string>? TestEnvironments { get; set; }
        public string? TestPlanKey { get; set; }
    }
}