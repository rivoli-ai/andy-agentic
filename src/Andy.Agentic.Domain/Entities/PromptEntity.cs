using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a prompt template associated with an agent.
/// </summary>
public class PromptEntity
{
    /// <summary>
    ///     Unique identifier for the prompt.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Prompt content or template text.
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the prompt is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     UTC timestamp when the prompt was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the prompt was last updated.
    /// </summary>
    [ConcurrencyCheck]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Foreign key referencing the owning agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Navigation property to the owning agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;

    /// <summary>
    ///     Collection of variables used by this prompt.
    /// </summary>
    public virtual ICollection<PromptVariableEntity> Variables { get; set; } = new List<PromptVariableEntity>();
}
