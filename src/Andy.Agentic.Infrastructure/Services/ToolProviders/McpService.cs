using System.Text.Json;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using ModelContextProtocol.Client;

namespace Andy.Agentic.Infrastructure.Services.ToolProviders;

/// <summary>
/// Service for discovering tools from MCP (Model Context Protocol) servers.
/// </summary>
public class McpService : IMcpService
{
    private readonly HttpClient _httpClient;

    public McpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Discovers available tools from an MCP server at the specified URL.
    /// </summary>
    /// <param name="serverUrl">The URL of the MCP server.</param>
    /// <returns>A response containing the discovered tools or error information.</returns>
    public async Task<McpToolDiscoveryResponse> DiscoverToolsAsync(string serverUrl)
    {
        try
        {
          
            var transport = CreateSseTransport(serverUrl);
            var client = await McpClientFactory.CreateAsync(transport).ConfigureAwait(false);

            var toolsResult = await client.ListToolsAsync().ConfigureAwait(false);

            var tools = new List<McpToolDiscovery>();

            if (toolsResult != null)
            {
                foreach (var tool in toolsResult)
                {
                    var mcpTool = new McpToolDiscovery
                    {
                        Name = tool.Name,
                        Description = tool.Description ?? string.Empty,
                        InputSchema = ParseInputSchema(tool.JsonSchema)
                    };
                    tools.Add(mcpTool);
                }
            }

            return new McpToolDiscoveryResponse
            {
                Success = true,
                Tools = tools
            };
        }
        catch (HttpRequestException ex)
        {
            return new McpToolDiscoveryResponse
            {
                Success = false,
                Error = $"Network error connecting to MCP server: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new McpToolDiscoveryResponse
            {
                Success = false,
                Error = $"Unexpected error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Converts an MCP tool discovery to a Tool entity.
    /// </summary>
    /// <param name="mcpTool">The discovered MCP tool.</param>
    /// <param name="serverUrl">The URL of the MCP server.</param>
    /// <returns>A Tool entity representing the MCP tool.</returns>
    public Tool ConvertToTool(McpToolDiscovery mcpTool, string serverUrl)
    {
        var parameters = new List<object>();
        
        if (mcpTool.InputSchema?.Properties != null)
        {
            foreach (var (key, property) in mcpTool.InputSchema.Properties)
            {
                var parameter = new
                {
                    name = key,
                    type = MapJsonTypeToParameterType(property.Type),
                    required = mcpTool.InputSchema.Required?.Contains(key) ?? false,
                    description = property.Description,
                    @default = property.Default,
                    @enum = property.Enum
                };
                parameters.Add(parameter);
            }
        }

        return new Tool
        {
            Id = Guid.NewGuid(),
            Name = mcpTool.Name,
            Description = mcpTool.Description,
            Type = "McpTool",
            Category = "MCP",
            IsActive = true,
            Configuration = JsonSerializer.Serialize(new { 
                serverUrl,
                mcpType = "sse",
                endpoint = serverUrl
            }),
            Parameters = JsonSerializer.Serialize(parameters),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublic = true
        };
    }

    /// <summary>
    /// Creates an SSE transport for MCP connection.
    /// </summary>
    /// <param name="endpoint">The MCP server endpoint URL.</param>
    /// <returns>An SSE client transport configured for the MCP server.</returns>
    private static IClientTransport CreateSseTransport(string endpoint)
    {
        var options = new SseClientTransportOptions
        {
            Endpoint = new Uri(endpoint),
            ConnectionTimeout = TimeSpan.FromSeconds(120),
            Name = "MCP Discovery Client"
        };

        return new SseClientTransport(options);
    }

    /// <summary>
    /// Parses the input schema from MCP tool definition.
    /// </summary>
    /// <param name="inputSchema">The input schema from MCP tool.</param>
    /// <returns>Parsed input schema or null if parsing fails.</returns>
    private static McpToolInputSchema? ParseInputSchema(object? inputSchema)
    {
        if (inputSchema == null) return null;

        try
        {
            var schemaJson = JsonSerializer.Serialize(inputSchema);
            var schemaElement = JsonDocument.Parse(schemaJson).RootElement;

            var schema = new McpToolInputSchema
            {
                Type = schemaElement.GetProperty("type").GetString() ?? "object"
            };

            if (schemaElement.TryGetProperty("properties", out var propertiesElement))
            {
                schema.Properties = new Dictionary<string, McpToolProperty>();
                foreach (var element in propertiesElement.EnumerateObject())
                {
                    schema.Properties[element.Name] = ParseProperty(element.Value);
                }
            }

            if (schemaElement.TryGetProperty("required", out var requiredElement) && requiredElement.ValueKind == JsonValueKind.Array)
            {
                schema.Required = new List<string>();
                foreach (var item in requiredElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        schema.Required.Add(item.GetString()!);
                    }
                }
            }

            return schema;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a property from the input schema.
    /// </summary>
    /// <param name="propertyElement">The JSON element representing the property.</param>
    /// <returns>Parsed property definition.</returns>
    private static McpToolProperty ParseProperty(JsonElement propertyElement)
    {
        var property = new McpToolProperty
        {
            Type = propertyElement.GetProperty("type").GetString() ?? "string"
        };

        if (propertyElement.TryGetProperty("description", out var descriptionElement))
        {
            property.Description = descriptionElement.GetString();
        }

        if (propertyElement.TryGetProperty("default", out var defaultElement))
        {
            property.Default = GetJsonValue(defaultElement);
        }

        if (propertyElement.TryGetProperty("enum", out var enumElement) && enumElement.ValueKind == JsonValueKind.Array)
        {
            property.Enum = new List<object>();
            foreach (var item in enumElement.EnumerateArray())
            {
                property.Enum.Add(GetJsonValue(item));
            }
        }

        if (propertyElement.TryGetProperty("items", out var itemsElement))
        {
            property.Items = ParseProperty(itemsElement);
        }

        return property;
    }

    /// <summary>
    /// Extracts a value from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element.</param>
    /// <returns>The extracted value.</returns>
    private static object GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Maps JSON schema types to parameter types.
    /// </summary>
    /// <param name="jsonType">The JSON schema type.</param>
    /// <returns>The mapped parameter type.</returns>
    private static string MapJsonTypeToParameterType(string jsonType)
    {
        return jsonType.ToLowerInvariant() switch
        {
            "string" => "string",
            "number" => "number",
            "integer" => "number",
            "boolean" => "boolean",
            "object" => "object",
            "array" => "array",
            _ => "string"
        };
    }
}
