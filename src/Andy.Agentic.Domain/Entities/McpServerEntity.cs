using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Entities;

/// <summary>
///     Represents a Model Context Protocol (MCP) server configuration usable by agents.
/// </summary>
public class McpServerEntity
{
    /// <summary>
    ///     Unique identifier for the MCP server.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Human-readable name of the server.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Description of the server capabilities.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Endpoint URL for accessing the server.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    ///     Network protocol used by the endpoint (default: http).
    /// </summary>
    [MaxLength(50)]
    public string? Protocol { get; set; } = "http";

    /// <summary>
    ///     Indicates whether the server configuration is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Serialized capabilities information.
    /// </summary>
    public string? Capabilities { get; set; }

    /// <summary>
    ///     Serialized configuration payload.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    ///     UTC timestamp when created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     UTC timestamp when last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
