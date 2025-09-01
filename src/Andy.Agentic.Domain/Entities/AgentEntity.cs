using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents an autonomous agent configuration, including metadata and relationships
///     to prompts, tools, MCP servers, and tags.
/// </summary>
public class AgentEntity
{
    /// <summary>
    ///     Unique identifier for the agent.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Human-readable name of the agent.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Optional description of the agent's purpose or behavior.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;


    /// <summary>
    ///     Agent type or category used for grouping/selection.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the agent is active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Number of times this agent has been executed.
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    ///     UTC timestamp when the agent was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the agent was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Foreign key referencing the associated LLM configuration.
    /// </summary>
    public Guid LlmConfigId { get; set; }

    /// <summary>
    ///     Navigation property to the LLM configuration used by this agent.
    /// </summary>
    public virtual LlmConfigEntity? LlmConfig { get; set; } = null!;

    /// <summary>
    ///     Collection of prompts associated with this agent.
    /// </summary>
    public virtual ICollection<PromptEntity> Prompts { get; set; } = new List<PromptEntity>();

    /// <summary>
    ///     Collection of tools available to this agent.
    /// </summary>
    public virtual ICollection<AgentToolEntity> Tools { get; set; } = new List<AgentToolEntity>();

    /// <summary>
    ///     Collection of MCP servers configured for this agent.
    /// </summary>
    public virtual ICollection<AgentMcpServerEntity> McpServers { get; set; } = new List<AgentMcpServerEntity>();

    /// <summary>
    ///     Collection of tags assigned to this agent.
    /// </summary>
    public virtual ICollection<AgentTagEntity> AgentTags { get; set; } = new List<AgentTagEntity>();
}
