namespace Testlemon.Core.Output.Models.XRay
{
    public class XRayImport
    {
        public required XRayInfo Info { get; set; }
        public required List<XRayTest> Tests { get; set; }
    }
}