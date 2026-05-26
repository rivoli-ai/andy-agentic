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

    /// <summary>
    /// Optional: SSE, HTTP Streaming, streamable-http, auto — drives MCP HTTP transport for discovery and stored tool config.
    /// </summary>
    public string? Transport { get; set; }

    /// <summary>
    /// Optional authentication JSON (same shape as tool <c>authentication</c>) sent as MCP HTTP headers during discovery.
    /// </summary>
    public string? Authentication { get; set; }

    /// <summary>
    /// Optional headers JSON (same shape as tool <c>headers</c>) sent as MCP HTTP headers during discovery.
    /// </summary>
    public string? Headers { get; set; }
}
