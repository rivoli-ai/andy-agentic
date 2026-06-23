using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents the many-to-many relationship between agents and documents.
/// </summary>
public class AgentDocumentEntity
{
    /// <summary>
    ///     Unique identifier for the agent-document relationship.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Foreign key referencing the agent.
    /// </summary>
    [Required]
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Navigation property to the agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;

    /// <summary>
    ///     Foreign key referencing the document.
    /// </summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>
    ///     Navigation property to the document.
    /// </summary>
    public virtual DocumentEntity Document { get; set; } = null!;

    /// <summary>
    ///     UTC timestamp when the relationship was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the relationship was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
