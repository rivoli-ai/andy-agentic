namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for AgentTag entities.
///     Represents the association between an agent and a tag.
/// </summary>
public class AgentTagDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the agent-tag association.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the ID of the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// Gets or sets the ID of the agent.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    ///     Gets or sets the tag.
    /// </summary>
    public virtual TagDto? Tag { get; set; } = null!;
}

