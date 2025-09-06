using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a tool that can be invoked by agents, including configuration and metadata.
/// </summary>
public class ToolEntity
{
    /// <summary>
    ///     Unique identifier for the tool.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Human-readable name of the tool.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Description of what the tool does.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Category/type of the tool (e.g., function, http, etc.).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Optional category used for grouping tools.
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    ///     Indicates whether the tool is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Optional configuration JSON or serialized settings.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    ///     Optional authentication data.
    /// </summary>
    public string? Authentication { get; set; }

    /// <summary>
    ///     Optional parameter schema or serialized parameters.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    ///     Optional headers schema or serialized headers.
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    ///     UTC timestamp when the tool was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the tool was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
