using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Join entity linking an agent to a specific skill (namespace/skill@version)
///     resolved from a configured skill registry. The full SKILL.md instructions are
///     fetched live from the registry; this row stores the coordinates plus a cached
///     name/description for the progressive-disclosure catalog.
/// </summary>
public class AgentSkillEntity
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
    ///     Navigation property to the agent.
    /// </summary>
    public virtual AgentEntity Agent { get; set; } = null!;

    /// <summary>
    ///     Foreign key referencing the skill registry connection the skill is resolved from.
    /// </summary>
    public Guid SkillRegistryId { get; set; }

    /// <summary>
    ///     Navigation property to the skill registry connection.
    /// </summary>
    public virtual SkillRegistryEntity Registry { get; set; } = null!;

    /// <summary>
    ///     Registry namespace slug (e.g. "acme").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    ///     Skill (package) slug (e.g. "pdf-filler").
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SkillSlug { get; set; } = string.Empty;

    /// <summary>
    ///     Semantic version of the attached skill (e.g. "1.2.0").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     Human-readable skill name shown to the model in the catalog.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Short skill description (from SKILL.md frontmatter) shown in the catalog.
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the association is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     UTC timestamp when the skill was attached.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
