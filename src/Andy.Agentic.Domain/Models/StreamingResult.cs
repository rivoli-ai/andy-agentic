using Andy.Agentic.Domain.Models;

public class StreamingResult
{
    public string? Content { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
    public string? AssistantMessage { get; set; }
    public List<string?> Messages { get; set; }
}
