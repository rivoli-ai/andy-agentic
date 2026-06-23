using Andy.Agentic.Domain.Models;

public class StreamingResult
{
    public string? Content { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
    public string? AssistantMessage { get; set; }
    public List<string?> Messages { get; set; }
    public string? Thinking { get; set; }

    /// <summary>Display labels of skills applied to this response (emitted once when known).</summary>
    public List<string>? SkillsUsed { get; set; }
}
