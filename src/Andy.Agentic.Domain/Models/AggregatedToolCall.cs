using System.Text;

namespace Andy.Agentic.Domain.Models;

/// <summary>
/// Aggregate Tool Call
/// </summary>
public sealed class AggregatedToolCall
{
    public int Index { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public StringBuilder ArgumentsBuffer { get; } = new StringBuilder();
    public string Arguments => ArgumentsBuffer.ToString();
}
