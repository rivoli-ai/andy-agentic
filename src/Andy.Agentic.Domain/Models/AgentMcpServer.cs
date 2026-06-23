using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Data Transfer Object for AgentMcpServer entities.
///     Represents the association between an agent and an MCP server.
/// </summary>
public class AgentMcpServer
{
    /// <summary>
    ///     Gets or sets the unique identifier for the agent-MCP server association.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the MCP server as configured for this agent.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether this MCP server is currently active for the agent.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the capabilities supported by this MCP server instance.
    /// </summary>
    public string? Capabilities { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the agent this MCP server is associated with.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Gets or sets the agent this MCP server is associated with.
    /// </summary>
    public virtual Agent Agent { get; set; } = null!;
}

