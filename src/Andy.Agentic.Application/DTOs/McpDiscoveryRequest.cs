using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Application.DTOs;

/// <summary>
/// Request DTO for MCP tool discovery.
/// </summary>
public class McpDiscoveryRequest
{
    /// <summary>
    /// Gets or sets the URL of the MCP server.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;
}
