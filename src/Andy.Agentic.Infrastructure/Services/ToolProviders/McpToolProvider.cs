using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using IMcpClient = ModelContextProtocol.Client.IMcpClient;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

public class McpToolProvider(ILogger<McpToolProvider> logger) : IToolProvider
{
    private readonly ILogger<McpToolProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string ToolType => "mcp";

    public async Task<object?> ExecuteToolAsync(Domain.Models.Tool tool, Dictionary<string, object> requestParameters)
    {
        if (tool == null)
            throw new ArgumentNullException(nameof(tool));

        IMcpClient? client = null;
        try
        {
            var configuration = ParseConfiguration(tool.Configuration);
            var auth = ParseAuthentication(tool.Authentication);

            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");
            var mcpType = GetConfigValue(configuration, "mcpType", "stdio");
            var toolName = GetRequiredConfigValue<string>(configuration, "name");


            client = await CreateMcpClientAsync(endpoint, mcpType, configuration, auth);

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
        string.Equals(toolType, ToolType, StringComparison.OrdinalIgnoreCase);

    private async Task<IMcpClient> CreateMcpClientAsync(
        string endpoint,
        string mcpType,
        Dictionary<string, object> configuration,
        Dictionary<string, object> auth)
    {
        IClientTransport transport = mcpType.ToLowerInvariant() switch
        {
            "stdio" => CreateStdioTransport(endpoint, configuration, auth),
            "sse" => CreateSseTransport(endpoint, configuration, auth),
            _ => throw new ArgumentException($"Unsupported MCP transport type: {mcpType}")
        };

        return await McpClientFactory.CreateAsync(transport);
    }

    private IClientTransport CreateStdioTransport(
        string endpoint,
        Dictionary<string, object> configuration,
        Dictionary<string, object> auth)
    {
        var commandParts = endpoint.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (commandParts.Length == 0)
            throw new ArgumentException("STDIO endpoint must contain command");

        var command = commandParts[0];
        var args = commandParts.Skip(1).ToArray();

        var workingDirectory = GetConfigValue<string>(configuration, "workingDirectory", null);
        var environmentVariables = GetConfigValue(configuration, "env", new Dictionary<string, string>());

        foreach (var kvp in auth)
            environmentVariables[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;

        var options = new StdioClientTransportOptions
        {
            Name = GetConfigValue(configuration, "name", "STDIO Client"),
            Command = command,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            EnvironmentVariables = environmentVariables
        };

        return new StdioClientTransport(options);
    }

    private IClientTransport CreateSseTransport(
        string endpoint,
        Dictionary<string, object> configuration,
        Dictionary<string, object> headers)
    {
        var options = new SseClientTransportOptions
        {
            Endpoint = new Uri(endpoint),
            Name = GetConfigValue(configuration, "name", "SSE Client"),
            AdditionalHeaders = headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty)
        };

        var timeout = GetConfigValue<TimeSpan?>(configuration, "timeout", null);
         if (timeout.HasValue)
            options.ConnectionTimeout = timeout.Value;

        return new SseClientTransport(options);
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
