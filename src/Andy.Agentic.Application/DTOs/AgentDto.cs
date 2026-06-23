using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for Agent entities.
///     Represents an AI agent with its configuration, tools, and capabilities.
/// </summary>
public class AgentDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the agent.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the agent.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the agent.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the type/category of the agent.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the agent is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the number of times this agent has been executed.
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    ///     Gets or sets when the agent was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the agent was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the ID of the user who created this agent.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Gets or sets whether this agent is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    ///     Gets or sets the ID of the associated LLM configuration.
    /// </summary>
    public Guid LlmConfigId { get; set; }

    /// <summary>
    ///     Gets or sets the associated LLM configuration.
    /// </summary>
    public virtual LlmConfigDto? LlmConfig { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the ID of the associated embedding LLM configuration for RAG.
    /// </summary>
    public Guid? EmbeddingLlmConfigId { get; set; }

    /// <summary>
    ///     Gets or sets the associated embedding LLM configuration for RAG.
    /// </summary>
    public virtual LlmConfigDto? EmbeddingLlmConfig { get; set; }

    /// <summary>
    ///     Gets or sets the collection of prompts associated with this agent.
    /// </summary>
    public virtual ICollection<PromptDto> Prompts { get; set; } = new List<PromptDto>();

    /// <summary>
    ///     Gets or sets the collection of tools associated with this agent.
    /// </summary>
    public virtual ICollection<AgentToolDto> Tools { get; set; } = new List<AgentToolDto>();

    /// <summary>
    ///     Gets or sets the collection of MCP servers associated with this agent.
    /// </summary>
    public virtual ICollection<AgentMcpServerDto> McpServers { get; set; } = new List<AgentMcpServerDto>();

    /// <summary>
    ///     Gets or sets the collection of tags associated with this agent.
    /// </summary>
    public virtual ICollection<AgentTagDto> AgentTags { get; set; } = new List<AgentTagDto>();

    /// <summary>
    ///     Gets or sets the collection of documents associated with this agent.
    /// </summary>
    public virtual ICollection<AgentDocumentDto> AgentDocuments { get; set; } = new List<AgentDocumentDto>();
}

