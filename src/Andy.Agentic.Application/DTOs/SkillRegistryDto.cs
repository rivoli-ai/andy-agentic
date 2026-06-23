using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for a skill registry connection. <see cref="AuthConfig"/> is
///     write-only: it is accepted on create/update but never returned in responses.
/// </summary>
public class SkillRegistryDto
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the connection name.</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the connection description.</summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the registry API base URL.</summary>
    [Required]
    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the authentication type: none | api_key | bearer | basic | oauth2.</summary>
    [MaxLength(50)]
    public string AuthType { get; set; } = "none";

    /// <summary>Gets or sets the serialized authentication configuration (write-only).</summary>
    public string? AuthConfig { get; set; }

    /// <summary>Gets or sets whether the connection is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets when the connection was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets when the connection was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
