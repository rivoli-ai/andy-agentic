using System.Text.Json;using Microsoft.SemanticKernel;using ModelContextProtocol.Client;using ModelContextProtocol.Protocol;using IMcpClient = ModelContextProtocol.Client.IMcpClient;using Tool = Andy.Agentic.Domain.Models.Tool;namespace Andy.Agentic.Infrastructure.Semantic.Tools;/// <summary>
/// Represents a factory for creating instances of MCP tools.
/// </summary>
public class McpToolFactory : ToolFactory{
    /// <summary>
    /// Creates a tool asynchronously using the provided tool configuration.
    /// </summary>
    /// <param name="tool">The tool configuration object containing details such as name, description, parameters, and authentication.</param>
    /// <returns>
    /// A KernelFunction object representing the created tool, which includes the method to call the tool dynamically and its metadata.
    /// </returns>
    public override KernelFunction CreateToolAsync(Tool tool)    {        IEnumerable<KernelParameterMetadata>? parameters = null;        async Task<string> DynamicMcpCall(KernelArguments args)        {
            var configuration = ParseConfiguration(tool.Configuration);            var headers = ParseHeaders(tool.Headers);

            var endpoint = GetRequiredConfigValue<string>(configuration, "endpoint");            var mcpType = GetConfigValue(configuration, "mcpType", "stdio");            var workingDirectory = GetConfigValue<string>(configuration, "workingDirectory", null!);

            var remoteToolName = GetConfigValue(configuration, "name", tool.Name);

            var client = await GetOrCreateClientAsync(endpoint, mcpType, workingDirectory, configuration, headers);

            var callArgs = ParseToolCallArguments(args);

            CallToolResult result;            try            {                result = await client.CallToolAsync(remoteToolName, callArgs!).ConfigureAwait(false);            }            catch (Exception ex)            {
                throw new InvalidOperationException($"MCP call failed for '{remoteToolName}': {ex.Message}", ex);            }

            return SerializeMcpResultToString(result);        }        if (!string.IsNullOrEmpty(tool.Parameters))        {            var paramSchema = ConvertParamsToDictionary(tool.Parameters);            parameters = paramSchema.Select(p =>            {                var metadata = new KernelParameterMetadata(p.Name)                {                    Description = $"Parameter for {p.Name}",                    ParameterType = p.Type,                    IsRequired = true                };                return metadata;            }).ToList();        }        return KernelFunctionFactory.CreateFromMethod(            method: DynamicMcpCall,            functionName: tool.Name,            description: tool.Description,            parameters: parameters        );    }    /// <summary>    /// Retrieves or creates an MCP client asynchronously based on the provided endpoint, MCP type, working directory, configuration, and headers.    /// Caches the client to avoid redundant creation.    /// </summary>    /// <param name="endpoint">The endpoint for the MCP client.</param>    /// <param name="mcpType">The type of MCP transport (e.g., "stdio", "sse").</param>    /// <param name="workingDirectory">The working directory for the MCP client, if applicable.</param>    /// <param name="configuration">A dictionary containing configuration settings for the MCP client.</param>    /// <param name="headers">A dictionary containing headers for the MCP client.</param>    /// <returns>    /// An asynchronous task that returns an instance of <see cref="IMcpClient"/>.    /// </returns>    // -------------------- MCP client creation & caching --------------------    private static async Task<IMcpClient> GetOrCreateClientAsync(        string endpoint,        string mcpType,        string? workingDirectory,        Dictionary<string, object> configuration,        Dictionary<string, object> headers)    {        var transport = mcpType.ToLowerInvariant() switch        {            "stdio" => CreateStdioTransport(endpoint, configuration, headers, workingDirectory),            "sse" => CreateSseTransport(endpoint, configuration, headers),            _ => throw new ArgumentException($"Unsupported MCP transport type: {mcpType}")        };        return await McpClientFactory.CreateAsync(transport).ConfigureAwait(false);    }    /// <summary>
    /// Creates an instance of IClientTransport using the provided endpoint, configuration, headers, and working directory.
    /// </summary>
    /// <param name="endpoint">The command and arguments to execute, formatted as "<command/> [args...]".</param>
    /// <param name="configuration">A dictionary containing configuration settings.</param>
    /// <param name="headers">A dictionary containing headers to be mapped to environment variables.</param>
    /// <param name="workingDirectory">The working directory for the command execution.</param>
    /// <returns>An instance of IClientTransport configured with the provided settings.</returns>
    private static IClientTransport CreateStdioTransport(
            string endpoint,
            Dictionary<string, object> configuration,
            Dictionary<string, object> headers,
            string? workingDirectory)    {
        // endpoint format: "<command> [args...]"
        var parts = endpoint.Split(' ', StringSplitOptions.RemoveEmptyEntries);        if (parts.Length == 0)        {
            throw new ArgumentException("STDIO endpoint must contain command");
        }        var command = parts[0];        var args = parts.Skip(1).ToArray();

        // Base env from config
        var env = GetConfigValue(configuration, "env", new Dictionary<string, string>());

        // Map headers to env vars (e.g., "Authorization" -> "AUTHORIZATION")
        foreach (var kvp in headers)        {            var envKey = kvp.Key.Replace('-', '_').ToUpperInvariant();            env[envKey] = kvp.Value.ToString() ?? string.Empty;        }        var options = new StdioClientTransportOptions        {            Name = GetConfigValue(configuration, "name", "STDIO Client"),            Command = command,            Arguments = args,            WorkingDirectory = workingDirectory,            EnvironmentVariables = env!        };        return new StdioClientTransport(options);    }

    /// <summary>
    /// Creates an instance of IClientTransport configured for Server-Sent Events (SSE).
    /// </summary>
    /// <param name="endpoint">The endpoint URL for the SSE connection.</param>
    /// <param name="configuration">A dictionary containing configuration settings.</param>
    /// <param name="headers">A dictionary containing headers to be included in the SSE connection.</param>
    /// <returns>An instance of IClientTransport configured for SSE.</returns>
    private static IClientTransport CreateSseTransport(
            string endpoint,
            Dictionary<string, object> configuration,
            Dictionary<string, object> headers)    {        var options = new SseClientTransportOptions        {            Endpoint = new Uri(endpoint),            ConnectionTimeout = TimeSpan.FromSeconds(120L),            Name = GetConfigValue(configuration, "name", "SSE Client"),            AdditionalHeaders = headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString() ?? string.Empty)        };        var timeout = GetConfigValue<TimeSpan?>(configuration, "timeout", null);        if (timeout.HasValue)        {
            options.ConnectionTimeout = timeout.Value;
        }        return new SseClientTransport(options);    }    /// <summary>    /// Serializes the result of a CallToolResult to a string.    /// </summary>    /// <param name="result">The CallToolResult to be serialized.</param>    /// <returns>    /// A string representation of the result. If the result has no content, returns an empty string.    /// If the result contains a single text block, returns the text directly.    /// Otherwise, returns a JSON string representing the content blocks.    /// </returns>    // -------------------- Result shaping --------------------    private static string SerializeMcpResultToString(CallToolResult result)    {        // No content -> empty        if (result.Content.Count == 0)        {            return string.Empty;        }        // Single text block -> return text directly        if (result.Content is [TextContentBlock tcb])        {            return tcb.Text;        }        // Otherwise serialize to JSON so the calling agent can parse richer output        var shaped = result.Content.Select<ContentBlock, object>(c => c switch        {            TextContentBlock t => new { type = "text", text = t.Text },            ImageContentBlock i => new { type = "image", data = i.Data, mimeType = i.MimeType },            // Add more MCP part types here as needed            _ => new { type = "unknown" }        }).ToList();        return JsonSerializer.Serialize(shaped);    }    /// <summary>
    /// Retrieves a configuration value from the provided dictionary. If the key is not found or the value is null, returns the default value.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <param name="cfg">The dictionary containing configuration values.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is null.</param>
    /// <returns>
    /// The configuration value if found and successfully converted; otherwise, the default value.
    /// </returns>
    private static T GetConfigValue<T>(Dictionary<string, object> cfg, string key, T defaultValue)    {        if (!cfg.TryGetValue(key, out var value))        {
            return defaultValue;
        }        try        {            return (T)ConvertTo(typeof(T), value)!;        }        catch        {            return defaultValue;        }    }

    /// <summary>
    /// Converts the given value to the specified target type.
    /// Handles conversion from JsonElement to various types including TimeSpan.
    /// </summary>
    /// <param name="targetType">The type to which the value should be converted.</param>
    /// <param name="value">The value to be converted.</param>
    /// <returns>
    /// The converted value as the specified target type, or null if conversion is not possible.
    /// </returns>
    private static object? ConvertTo(Type targetType, object value)    {        if (value is JsonElement el)        {            return el.ValueKind switch            {                JsonValueKind.String => targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?)                    ? TimeSpan.Parse(el.GetString()!)                    : Convert.ChangeType(el.GetString(), targetType),                JsonValueKind.Number => Convert.ChangeType(el.GetDouble(), targetType),                JsonValueKind.True => Convert.ChangeType(true, targetType),                JsonValueKind.False => Convert.ChangeType(false, targetType),                _ => null            };        }        if (targetType != typeof(TimeSpan) && targetType != typeof(TimeSpan?))        {            return Convert.ChangeType(value, targetType);        }        if (value is string s && TimeSpan.TryParse(s, out var ts))        {
            return ts;
        }        return Convert.ChangeType(value, targetType);    }}