namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for a skill attached to an agent.
/// </summary>
public class AgentSkillDto
{
    /// <summary>Gets or sets the association identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the agent identifier.</summary>
    public Guid AgentId { get; set; }

    /// <summary>Gets or sets the registry connection identifier.</summary>
    public Guid SkillRegistryId { get; set; }

    /// <summary>Gets or sets the registry namespace slug.</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>Gets or sets the skill (package) slug.</summary>
    public string SkillSlug { get; set; } = string.Empty;

    /// <summary>Gets or sets the semantic version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable skill name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the short skill description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the association is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets when the skill was attached.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
