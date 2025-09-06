using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a single chat message exchanged within a session, optionally linked to an agent and tool execution.
/// </summary>
public class ChatMessageEntity
{
    /// <summary>
    ///     Unique identifier for the message.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Message content.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Role of the sender (e.g., user, assistant, system).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    ///     Optional agent name if relevant to the message.
    /// </summary>
    [MaxLength(100)]
    public string? AgentName { get; set; }

    /// <summary>
    ///     Timestamp when the message was created (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Chat session identifier.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    ///     Optional identifier of the related agent.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    ///     Number of tokens used in this message, if known.
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    ///     Indicates whether this message represents a tool execution event.
    /// </summary>
    public bool IsToolExecution { get; set; } = false;

    /// <summary>
    ///     Name of the tool executed, if applicable.
    /// </summary>
    [MaxLength(100)]
    public string? ToolName { get; set; }

    /// <summary>
    ///     Result of tool execution, if applicable.
    /// </summary>
    public string? ToolResult { get; set; }

    /// <summary>
    ///     Navigation property to the related agent.
    /// </summary>
    public virtual AgentEntity? Agent { get; set; }

    /// <summary>
    ///     Tool Execution Results
    /// </summary>

    public List<ToolExecutionLogEntity> ToolResults { get; set; } = new();
}
