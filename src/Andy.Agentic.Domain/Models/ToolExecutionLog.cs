using Andy.Agentic.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Data Transfer Object for ToolExecutionLog entities.
///     Represents a log entry for tool execution by agents.
/// </summary>
public class ToolExecutionLog
{
    /// <summary>
    ///     Gets or sets the unique identifier for the tool execution log.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the tool that was executed.
    /// </summary>
    [Required]
    public Guid ToolId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the tool that was executed.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the ID of the agent that executed the tool.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    ///     Gets or sets the agent that executed the tool.
    /// </summary>
    public virtual Agent? Agent { get; set; }

    /// <summary>
    ///     Gets or sets the session identifier for grouping related executions.
    /// </summary>
    [MaxLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    ///     Gets or sets the parameters used for the tool execution.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    ///     Gets or sets the result of the tool execution.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    ///     Gets or sets whether the tool execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets the error message if the execution failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Gets or sets when the tool execution occurred.
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the execution time in milliseconds.
    /// </summary>
    public double ExecutionTime { get; set; }

    /// <summary>
    ///     Identifier of the executed tool.
    /// </summary>
    [Required]
    public Tool Tool { get; set; }

}

