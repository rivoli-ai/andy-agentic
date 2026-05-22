using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

/// <summary>
/// Service for discovering tools from MCP (Model Context Protocol) servers.
/// </summary>
public interface IMcpService
{
    /// <summary>
    /// Discovers available tools from an MCP server at the specified URL.
    /// </summary>
    /// <param name="serverUrl">The URL of the MCP server.</param>
    /// <returns>A response containing the discovered tools or error information.</returns>
    Task<McpToolDiscoveryResponse> DiscoverToolsAsync(
        string serverUrl,
        string? transport = null,
        string? authentication = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts an MCP tool discovery to a Tool entity.
    /// </summary>
    /// <param name="mcpTool">The discovered MCP tool.</param>
    /// <param name="serverUrl">The URL of the MCP server.</param>
    /// <param name="transport">UI transport label (e.g. SSE, HTTP Streaming); stored on the tool for execution.</param>
    /// <returns>A Tool entity representing the MCP tool.</returns>
    Tool ConvertToTool(McpToolDiscovery mcpTool, string serverUrl, string? transport = null);
}
