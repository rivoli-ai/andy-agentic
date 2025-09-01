using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Join entity linking an agent to a tool and its active status.
/// </summary>
public class AgentToolEntity
{
    /// <summary>
    ///     Indicates whether the agent has this tool enabled.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Optional foreign key referencing the tool.
    /// </summary>
    public Guid? ToolId { get; set; }

    /// <summary>
    ///     Foreign key referencing the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Navigation property to the agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;

    /// <summary>
    ///     Navigation property to the tool.
    /// </summary>
    public virtual ToolEntity Tool { get; set; } = null!;

    /// <summary>
    ///     UTC timestamp when the association was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the association was last updated.
    /// </summary>
    [ConcurrencyCheck]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
