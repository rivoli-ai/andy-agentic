using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for PromptVariable entities.
///     Represents a variable that can be used within a prompt template.
/// </summary>
public class PromptVariableDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the prompt variable.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the variable.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the data type of the variable.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the variable is required for the prompt.
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    ///     Gets or sets the default value for the variable.
    /// </summary>
    [MaxLength(500)]
    public string? DefaultValue { get; set; }

    /// <summary>
    ///     Gets or sets the description of the variable.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the prompt this variable belongs to.
    /// </summary>
    public Guid PromptId { get; set; }

    /// <summary>
    ///     Gets or sets the prompt this variable belongs to.
    /// </summary>
    public virtual PromptDto Prompt { get; set; } = null!;
}

