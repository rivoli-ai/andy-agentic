using System.Collections.Concurrent;

namespace Andy.Agentic.Domain.Models;

public sealed class ToolExecutionRecorder
{
    private readonly ConcurrentBag<ToolExecutionLog> _records = new();
    public IReadOnlyCollection<ToolExecutionLog> Records => _records;
    public void Add(ToolExecutionLog r) => _records.Add(r);
}
