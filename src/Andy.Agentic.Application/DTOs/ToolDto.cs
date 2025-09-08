using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for Tool entities.
///     Represents a tool that can be used by agents for various operations.
/// </summary>
public class ToolDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the tool.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the tool.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the tool.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type of the tool.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the category of the tool.
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    ///     Gets or sets whether the tool is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the configuration settings for the tool.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    ///     Gets or sets the authentication settings for the tool.
    /// </summary>
    public string? Authentication { get; set; }

    /// <summary>
    ///     Gets or sets the parameters schema for the tool.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    ///     Gets or sets the headers schema for the tool.
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    ///     Gets or sets when the tool was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the tool was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the ID of the user who created this tool.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Gets or sets whether this tool is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;
}

