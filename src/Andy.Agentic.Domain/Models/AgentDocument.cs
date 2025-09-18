using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Represents the many-to-many relationship between agents and documents.
/// </summary>
public class AgentDocument
{
    /// <summary>
    ///     Gets or sets the unique identifier for the agent-document relationship.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the agent.
    /// </summary>
    [Required]
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the document.
    /// </summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>
    ///     Gets or sets the associated document.
    /// </summary>
    public virtual Document? Document { get; set; }

    /// <summary>
    ///     Gets or sets when the relationship was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the relationship was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
