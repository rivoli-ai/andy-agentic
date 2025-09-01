using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for ChatMessage entities.
///     Represents a message in a chat conversation with an agent.
/// </summary>
public class ChatMessageDto
{
    [Required] public string Content { get; set; } = string.Empty;

    [Required] public Guid? AgentId { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string Role { get; set; } = "user";

    public int? TokenCount { get; set; }

    public bool IsToolExecution { get; set; }

    public string? ToolName { get; set; }

    public string? ToolResult { get; set; }
}

