using System.Collections.Concurrent;
using Testlemon.Core.Models.DFS;

namespace Testlemon.Core.DFS
{
    public class NodeProcessor<T, R>
        where T : INode
    {
        private readonly ConcurrentDictionary<string, T> _nodesDictionary = new();
        private readonly ConcurrentDictionary<string, Task<R?>> _tasksDictionary = new();

        public IEnumerable<Task<R?>> ProcessNodes(IEnumerable<T> nodes, Func<T, Task<R?>> func, bool parallel)
        {
            // Populate the dictionary
            foreach (var node in nodes)
            {
                if (!string.IsNullOrWhiteSpace(node.Id))
                {
                    _nodesDictionary[node.Id] = node;
                }
            }

            // Process nodes
            var tasks = new List<Task<R?>>();

            foreach (var node in nodes)
            {
                var task = ProcessNodeAsync(node, func);

                if (parallel)
                {
                    tasks.Add(task);
                }
                else
                {
                    task.Wait();
                    tasks.Add(Task.FromResult(task.Result));
                }
            }

            return tasks;
        }

        private async Task<R?> ProcessNodeAsync(T node, Func<T, Task<R?>> func)
        {
            if (!string.IsNullOrWhiteSpace(node.Id) && _tasksDictionary.TryGetValue(node.Id, out Task<R?>? value))
            {
                return await value;
            }

            var task = ProcessNodeInternalAsync(node, func);
            if (!string.IsNullOrWhiteSpace(node.Id))
            {
                _tasksDictionary[node.Id] = task;
            }

            return await task;
        }

        private async Task<R?> ProcessNodeInternalAsync(T node, Func<T, Task<R?>> func)
        {
            // If the node has a parent, process the parent first
            if (!string.IsNullOrWhiteSpace(node.DependsOn) && _nodesDictionary.TryGetValue(node.DependsOn, out T? value))
            {
                await ProcessNodeAsync(value, func);
            }

            // Process the current node
            return await func(node);
        }
    }
}