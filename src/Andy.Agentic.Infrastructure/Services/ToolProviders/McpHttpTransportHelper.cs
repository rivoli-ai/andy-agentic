using ModelContextProtocol.Client;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Maps UI / config MCP transport names to <see cref="HttpTransportMode" /> and canonical config strings.
/// </summary>
internal static class McpHttpTransportHelper
{
    /// <summary>
    /// Discovery: explicit UI transport wins; otherwise infer from endpoint path (<c>/mcp</c>, <c>/sse</c>).
    /// </summary>
    public static HttpTransportMode GetModeForDiscovery(string? transportFromUi, string? endpointUrl = null)
    {
        var normalized = Normalize(transportFromUi);
        if (normalized is "sse")
            return HttpTransportMode.Sse;
        if (normalized is "httpstreaming" or "streamablehttp")
            return HttpTransportMode.StreamableHttp;

        return InferModeFromEndpoint(endpointUrl) ?? HttpTransportMode.AutoDetect;
    }

    /// <summary>
    /// Execution: resolves stored <c>mcpType</c> from tool configuration JSON, with endpoint fallback.
    /// </summary>
    public static HttpTransportMode GetModeForExecution(string? storedMcpType, string? endpointUrl = null)
    {
        var normalized = Normalize(storedMcpType);
        if (normalized is "sse")
            return HttpTransportMode.Sse;
        if (normalized is "httpstreaming" or "streamablehttp")
            return HttpTransportMode.StreamableHttp;

        if (normalized is "" or "auto" or "autodetect")
            return InferModeFromEndpoint(endpointUrl) ?? HttpTransportMode.AutoDetect;

        return HttpTransportMode.AutoDetect;
    }

    public static bool IsStdio(string? mcpType) =>
        Normalize(mcpType) == "stdio";

    /// <summary>
    /// Value persisted in tool configuration so execution matches the transport used at discovery time.
    /// </summary>
    public static string ToStorageTransport(string? transportFromUi, string? endpointUrl = null)
    {
        var normalized = Normalize(transportFromUi);
        return normalized switch
        {
            "httpstreaming" or "streamablehttp" => "streamable-http",
            "sse" => "sse",
            "auto" or "autodetect" or "" => InferStorageFromEndpoint(endpointUrl) ?? "auto",
            _ => InferStorageFromEndpoint(endpointUrl) ?? "auto"
        };
    }

    internal static HttpTransportMode? InferModeFromEndpoint(string? endpointUrl)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl) || !Uri.TryCreate(endpointUrl.Trim(), UriKind.Absolute, out var uri))
            return null;

        var path = uri.AbsolutePath.TrimEnd('/').ToLowerInvariant();
        if (path.EndsWith("/mcp", StringComparison.Ordinal))
            return HttpTransportMode.StreamableHttp;
        if (path.EndsWith("/sse", StringComparison.Ordinal))
            return HttpTransportMode.Sse;

        return null;
    }

    private static string? InferStorageFromEndpoint(string? endpointUrl) =>
        InferModeFromEndpoint(endpointUrl) switch
        {
            HttpTransportMode.StreamableHttp => "streamable-http",
            HttpTransportMode.Sse => "sse",
            _ => null
        };

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty);
    }
}
