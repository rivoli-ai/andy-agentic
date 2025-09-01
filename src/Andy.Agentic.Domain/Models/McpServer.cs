using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Data Transfer Object for MCP (Model Context Protocol) server entities.
///     Represents an MCP server that can be used by agents for external communication.
/// </summary>
public class McpServer
{
    /// <summary>
    ///     Gets or sets the unique identifier for the MCP server.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the MCP server.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the MCP server.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the endpoint URL for the MCP server.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the protocol used by the MCP server (default: "http").
    /// </summary>
    [MaxLength(50)]
    public string? Protocol { get; set; } = "http";

    /// <summary>
    ///     Gets or sets whether the MCP server is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets the capabilities supported by the MCP server.
    /// </summary>
    public string? Capabilities { get; set; }

    /// <summary>
    ///     Gets or sets the configuration settings for the MCP server.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    ///     Gets or sets when the MCP server was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets when the MCP server was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

