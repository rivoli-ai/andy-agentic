using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for Tag entities.
///     Represents a tag that can be associated with agents for categorization.
/// </summary>
public class TagDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the tag.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the tag.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the tag.
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the color associated with the tag for UI display.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    ///     Gets or sets when the tag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

