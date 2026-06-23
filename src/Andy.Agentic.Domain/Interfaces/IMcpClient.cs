using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
/// Interface for MCP (Model Context Protocol) client
/// </summary>
public interface IMcpClient : IDisposable
{
    /// <summary>
    /// Connects to the MCP server
    /// </summary>
    /// <param name="endpoint">The MCP server endpoint</param>
    /// <param name="mcpType">The MCP connection type (SSE or HTTP Streaming)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    Task ConnectAsync(string endpoint, string mcpType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the MCP session
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization operation</returns>
    Task<McpInitializeResult> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available tools from the MCP server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available tools</returns>
    Task<List<McpTool>> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls a tool on the MCP server
    /// </summary>
    /// <param name="toolName">Name of the tool to call</param>
    /// <param name="arguments">Arguments for the tool call</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tool call result</returns>
    Task<McpToolCallResult> CallToolAsync(string toolName, Dictionary<string, object> arguments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the MCP server
    /// </summary>
    /// <returns>Task representing the disconnection operation</returns>
    Task DisconnectAsync();

    /// <summary>
    /// Gets whether the client is connected
    /// </summary>
    bool IsConnected { get; }
}
