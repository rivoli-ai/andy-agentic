using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Data Transfer Object for Prompt entities.
///     Represents a prompt template that can be used by agents.
/// </summary>
public class Prompt
{
    /// <summary>
    ///     Gets or sets the unique identifier for the prompt.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the content of the prompt template.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the prompt is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets when the prompt was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the prompt was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the ID of the agent this prompt belongs to.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Gets or sets the collection of variables associated with this prompt.
    /// </summary>
    public virtual ICollection<PromptVariable> Variables { get; set; } = new List<PromptVariable>();
}

