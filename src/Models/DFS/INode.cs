namespace Testlemon.Core.Models.DFS
{
    public interface INode
    {
        string? Id { get; set; }

        string? DependsOn { get; set; }
    }
}