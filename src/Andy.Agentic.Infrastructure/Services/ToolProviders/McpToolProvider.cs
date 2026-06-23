using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

public class McpToolProvider(ILogger<McpToolProvider> logger, HttpClient httpClient) : IToolProvider
{
    private readonly ILogger<McpToolProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public string ToolType => "mcp";

    public async Task<object?> ExecuteToolAsync(Domain.Models.Tool tool, Dictionary<string, object> requestParameters)
    {
        if (tool == null)
            throw new ArgumentNullException(nameof(tool));

        McpClient? client = null;
        try
        {
            var configuration = ParseConfiguration(tool.Configuration);
            var auth = ParseAuthentication(tool.Authentication);

            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");
            var mcpType = GetConfigValue(configuration, "mcpType", "auto");
            var toolName = GetRequiredConfigValue<string>(configuration, "name");

            client = await CreateMcpClientAsync(endpoint, mcpType, configuration, tool.Headers, auth);

            var result = await client.CallToolAsync(toolName, requestParameters);

            logger.LogInformation("Successfully executed MCP tool '{ToolName}' on endpoint '{Endpoint}'",
                toolName, endpoint);

            return ExtractContentFromResult(result);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Failed to execute MCP tool '{ToolName}': {ErrorMessage}",
                tool.Name, ex.Message);
            throw new InvalidOperationException($"Failed to execute MCP tool '{tool.Name}': {ex.Message}", ex);
        }
    }

    public bool CanHandleToolType(string toolType) =>
        ToolTypeHelper.Matches(Domain.Models.Semantic.ToolType.McpTool, toolType);

    private async Task<McpClient> CreateMcpClientAsync(
        string endpoint,
        string mcpType,
        Dictionary<string, object> configuration,
        string? headersFromTool,
        Dictionary<string, object> auth,
        CancellationToken cancellationToken = default)
    {
        var toolHeaders = ToolHeadersParser.Parse(headersFromTool);
        var httpHeaders = new Dictionary<string, string>(toolHeaders, StringComparer.OrdinalIgnoreCase);
        var authHeaders = await ToolAuthHeaderBuilder
            .BuildHeadersAsync(auth, _httpClient, cancellationToken)
            .ConfigureAwait(false);

        foreach (var (key, value) in authHeaders)
            httpHeaders.TryAdd(key, value);

        _logger.LogDebug(
            "MCP client headers for endpoint {Endpoint}: {HeaderCount} total ({ToolHeaderCount} from tool.Headers)",
            endpoint,
            httpHeaders.Count,
            toolHeaders.Count);

        IClientTransport transport = McpHttpTransportHelper.IsStdio(mcpType)
            ? CreateStdioTransport(endpoint, configuration, httpHeaders)
            : CreateHttpTransport(endpoint, configuration, httpHeaders, McpHttpTransportHelper.GetModeForExecution(mcpType, endpoint));

        return await McpClient.CreateAsync(transport, new McpClientOptions(), NullLoggerFactory.Instance, cancellationToken);
    }

    private static IClientTransport CreateStdioTransport(
        string endpoint,
        Dictionary<string, object> configuration,
        IReadOnlyDictionary<string, string> headers)
    {
        var commandParts = endpoint.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (commandParts.Length == 0)
            throw new ArgumentException("STDIO endpoint must contain command");

        var command = commandParts[0];
        var args = commandParts.Skip(1).ToArray();

        var workingDirectory = GetConfigValue<string>(configuration, "workingDirectory", null);
        var environmentVariables = GetConfigValue(configuration, "env", new Dictionary<string, string>());

        ToolHeadersParser.ApplyToEnvironmentVariables(headers, environmentVariables);

        var options = new StdioClientTransportOptions
        {
            Name = GetConfigValue(configuration, "name", "STDIO Client"),
            Command = command,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            EnvironmentVariables = environmentVariables
        };

        return new StdioClientTransport(options, NullLoggerFactory.Instance);
    }

    private static IClientTransport CreateHttpTransport(
        string endpoint,
        Dictionary<string, object> configuration,
        IReadOnlyDictionary<string, string> httpHeaders,
        HttpTransportMode transportMode)
    {
        var timeout = GetConfigValue<TimeSpan?>(configuration, "timeout", null);
        return McpHttpTransportFactory.Create(
            endpoint,
            transportMode,
            httpHeaders,
            GetConfigValue(configuration, "name", "MCP HTTP Client"),
            timeout);
    }


    private static Task HandleAuthorizationUrlAsync(Uri url, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Please open the following URL in your browser to authorize: {url}");
        return Task.CompletedTask;
    }

    private static object? ExtractContentFromResult(CallToolResult result)
    {
        if (result.Content == null || !result.Content.Any())
            return null;

        if (result.Content.Count == 1 && result.Content.First() is TextContentBlock textContent)
            return textContent.Text;

        return result.Content.Select<ContentBlock, object>(content => content switch
        {
            TextContentBlock text => new { type = "text", text = text.Text },
            ImageContentBlock image => new { type = "image", data = image.Data, mimeType = image.MimeType },
            _ => new { type = "unknown", content }
        }).ToList();
    }

    private static Dictionary<string, object> ParseConfiguration(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(configuration)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool configuration", nameof(configuration), ex);
        }
    }

    private static Dictionary<string, object> ParseAuthentication(string? authentication)
    {
        if (string.IsNullOrEmpty(authentication))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(authentication)
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON in tool authentication", nameof(authentication), ex);
        }
    }

    private static T GetRequiredConfigValue<T>(Dictionary<string, object> configuration, string key)
    {
        if (!configuration.TryGetValue(key, out var value))
            throw new ArgumentException($"Required configuration key '{key}' not found");

        if (value is JsonElement jsonElement)
            return jsonElement.Deserialize<T>() ?? throw new ArgumentException($"Invalid value for configuration key '{key}'");

        if (value is T directValue)
            return directValue;

        throw new ArgumentException($"Invalid value type for configuration key '{key}'");
    }

    private static T GetConfigValue<T>(Dictionary<string, object> configuration, string key, T defaultValue)
    {
        if (!configuration.TryGetValue(key, out var value))
            return defaultValue;

        if (value is JsonElement jsonElement)
            return jsonElement.Deserialize<T>() ?? defaultValue;

        if (value is T directValue)
            return directValue;

        return defaultValue;
    }
}
