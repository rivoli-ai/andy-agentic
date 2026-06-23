using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a variable used within a prompt template.
/// </summary>
public class PromptVariableEntity
{
    /// <summary>
    ///     Unique identifier for the prompt variable.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Variable name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Variable type (e.g., string, number).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the variable is required.
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    ///     Optional default value for the variable.
    /// </summary>
    [MaxLength(500)]
    public string? DefaultValue { get; set; }

    /// <summary>
    ///     Optional description of the variable.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Foreign key referencing the owning prompt.
    /// </summary>
    public Guid PromptId { get; set; }

    /// <summary>
    ///     Navigation property to the owning prompt.
    /// </summary>
    public virtual PromptEntity Prompt { get; set; } = null!;
}
