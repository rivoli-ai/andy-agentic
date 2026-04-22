using ModelContextProtocol.Client;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Maps UI / config MCP transport names to <see cref="HttpTransportMode" /> and canonical config strings.
/// </summary>
internal static class McpHttpTransportHelper
{
    /// <summary>
    /// Discovery: empty or unknown uses AutoDetect so both /mcp and /sse style URLs can work.
    /// </summary>
    public static HttpTransportMode GetModeForDiscovery(string? transportFromUi) =>
        Normalize(transportFromUi) switch
        {
            "sse" => HttpTransportMode.Sse,
            "httpstreaming" or "streamablehttp" => HttpTransportMode.StreamableHttp,
            _ => HttpTransportMode.AutoDetect
        };

    /// <summary>
    /// Execution: resolves stored <c>mcpType</c> from tool configuration JSON.
    /// </summary>
    public static HttpTransportMode GetModeForExecution(string? storedMcpType) =>
        Normalize(storedMcpType) switch
        {
            "sse" => HttpTransportMode.Sse,
            "streamablehttp" or "httpstreaming" => HttpTransportMode.StreamableHttp,
            _ => HttpTransportMode.AutoDetect
        };

    public static bool IsStdio(string? mcpType) =>
        Normalize(mcpType) == "stdio";

    /// <summary>
    /// Value persisted in tool configuration so execution matches the transport used at discovery time.
    /// </summary>
    public static string ToStorageTransport(string? transportFromUi) =>
        Normalize(transportFromUi) switch
        {
            "httpstreaming" or "streamablehttp" => "streamable-http",
            "auto" or "autodetect" => "auto",
            "sse" => "sse",
            "" => "auto",
            _ => "auto"
        };

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
    }
}
