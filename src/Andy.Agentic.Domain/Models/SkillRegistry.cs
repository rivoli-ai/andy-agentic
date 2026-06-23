using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Domain model for a connection to an external agent-skill registry.
/// </summary>
public class SkillRegistry
{
    /// <summary>
    ///     Gets or sets the unique identifier for the registry connection.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the registry connection.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the registry connection.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the base URL of the registry API.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the authentication type: none | api_key | bearer | basic | oauth2.
    /// </summary>
    [MaxLength(50)]
    public string AuthType { get; set; } = "none";

    /// <summary>
    ///     Gets or sets the serialized authentication configuration (JSON).
    /// </summary>
    public string? AuthConfig { get; set; }

    /// <summary>
    ///     Gets or sets whether the registry connection is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets when the connection was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the connection was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
