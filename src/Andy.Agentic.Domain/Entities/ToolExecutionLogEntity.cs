using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a log entry for a tool execution, including parameters, results, and timing.
/// </summary>
public class ToolExecutionLogEntity
{
    /// <summary>
    ///     Unique identifier for the log entry.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Identifier of the executed tool.
    /// </summary>
    [Required]
    public Guid ToolId { get; set; }


    /// <summary>
    ///     Identifier of the executed tool.
    /// </summary>
    [Required]
    public ToolEntity Tool { get; set; }

    /// <summary>
    ///     Name of the executed tool.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ToolName { get; set; } = string.Empty;


    /// <summary>
    ///     Optional identifier of the related agent.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    ///     Navigation to the related agent, if any.
    /// </summary>
    public virtual AgentEntity? Agent { get; set; }

    /// <summary>
    ///     Optional chat session identifier.
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    ///     Foreign key referencing the user who executed this tool.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Navigation property to the user who executed this tool.
    /// </summary>
    public virtual UserEntity? User { get; set; }

    /// <summary>
    ///     Serialized parameters passed to the tool.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    ///     Serialized result returned by the tool.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    ///     Indicates whether the execution succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Optional error message if the execution failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     UTC timestamp when the execution occurred.
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Duration of the execution in seconds.
    /// </summary>
    public double ExecutionTime { get; set; }
}
