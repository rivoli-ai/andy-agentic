using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for ChatHistory entities.
///     Represents a chat conversation history entry with performance metrics.
/// </summary>
public class ChatHistoryDto
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
    public Guid? UserId { get; set; }

    public List<ToolExecutionLogDto> ToolResults { get; set; } = new();
}

