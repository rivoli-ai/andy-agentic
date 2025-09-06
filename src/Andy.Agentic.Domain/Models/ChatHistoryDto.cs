using Andy.Agentic.Domain.Models;

public class ChatHistory
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string AgentName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid AgentId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public bool IsToolExecution { get; set; }
    public string? ToolName { get; set; }
    public string? ToolResult { get; set; }

    public List<ToolExecutionLog> ToolResults { get; set; } = new();
}
