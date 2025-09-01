using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Join entity associating agents with tags.
/// </summary>
public class AgentTagEntity
{
    /// <summary>
    ///     Unique identifier for the association.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Foreign key referencing the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Foreign key referencing the tag.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    ///     Navigation property to the tag.
    /// </summary>
    public virtual TagEntity Tag { get; set; } = null!;

    /// <summary>
    ///     Navigation property to the agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;
}
