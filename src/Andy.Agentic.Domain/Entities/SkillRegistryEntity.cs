using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a connection to an external agent-skill registry (e.g. andy-skills)
///     from which skills can be searched and attached to agents.
/// </summary>
public class SkillRegistryEntity
{
    /// <summary>
    ///     Unique identifier for the skill registry connection.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Human-readable name of the registry connection.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Optional description of the registry connection.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Base URL of the registry API (e.g. http://localhost:8080).
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Authentication type: none | api_key | bearer | basic | oauth2.
    /// </summary>
    [MaxLength(50)]
    public string AuthType { get; set; } = "none";

    /// <summary>
    ///     Serialized authentication configuration (JSON). Shape mirrors the
    ///     API-tool authentication payload consumed by HttpAuthApplier.
    /// </summary>
    public string? AuthConfig { get; set; }

    /// <summary>
    ///     Indicates whether the registry connection is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     UTC timestamp when the connection was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when the connection was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
