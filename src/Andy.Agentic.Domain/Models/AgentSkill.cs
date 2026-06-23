namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Domain model for a skill attached to an agent (namespace/skill@version) resolved
///     from a configured skill registry.
/// </summary>
public class AgentSkill
{
    /// <summary>
    ///     Gets or sets the unique identifier for the association.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the agent identifier.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    ///     Gets or sets the skill registry connection identifier.
    /// </summary>
    public Guid SkillRegistryId { get; set; }

    /// <summary>
    ///     Gets or sets the registry connection this skill is resolved from.
    /// </summary>
    public virtual SkillRegistry? Registry { get; set; }

    /// <summary>
    ///     Gets or sets the registry namespace slug.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the skill (package) slug.
    /// </summary>
    public string SkillSlug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the semantic version of the attached skill.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the human-readable skill name shown in the catalog.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the short skill description shown in the catalog.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the association is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets when the skill was attached.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
