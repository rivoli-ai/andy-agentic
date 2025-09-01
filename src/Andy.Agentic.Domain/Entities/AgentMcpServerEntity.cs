using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Join entity linking an agent to an MCP server and its capabilities.
/// </summary>
public class AgentMcpServerEntity
{
    /// <summary>
    ///     Unique identifier for the association.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Association name or alias for the MCP server.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the association is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Optional serialized capabilities specific to this agent-server link.
    /// </summary>
    public string? Capabilities { get; set; }

    /// <summary>
    ///     Foreign key referencing the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Navigation property to the agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;
}
