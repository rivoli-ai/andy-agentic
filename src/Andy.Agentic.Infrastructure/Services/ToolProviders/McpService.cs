using System.Text.Json;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
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
    public async Task<McpToolDiscoveryResponse> DiscoverToolsAsync(
        string serverUrl,
        string? transport = null,
        string? authentication = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mode = McpHttpTransportHelper.GetModeForDiscovery(transport, serverUrl);
            var authHeaders = await ToolAuthHeaderBuilder
                .BuildHeadersAsync(authentication, _httpClient, cancellationToken)
                .ConfigureAwait(false);
            var clientTransport = McpHttpTransportFactory.Create(serverUrl, mode, authHeaders, "MCP Discovery Client");
            await using var client = await McpClient
                .CreateAsync(clientTransport, new McpClientOptions(), NullLoggerFactory.Instance, default)
                .ConfigureAwait(false);

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
    public Tool ConvertToTool(McpToolDiscovery mcpTool, string serverUrl, string? transport = null)
    {
        var storedTransport = McpHttpTransportHelper.ToStorageTransport(transport, serverUrl);

        return new Tool
        {
            Id = Guid.NewGuid(),
            Name = mcpTool.Name,
            Description = mcpTool.Description,
            Type = "mcp",
            Category = "MCP",
            IsActive = true,
            Configuration = JsonSerializer.Serialize(new { 
                serverUrl,
                mcpType = storedTransport,
                endpoint = serverUrl,
                name = mcpTool.Name
            }),
            Parameters = BuildStoredParametersJson(mcpTool.InputSchema),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublic = true
        };
    }

    private static string BuildStoredParametersJson(McpToolInputSchema? inputSchema)
    {
        if (inputSchema?.Properties is not { Count: > 0 })
        {
            return "[]";
        }

        var properties = new Dictionary<string, object>();
        foreach (var (key, property) in inputSchema.Properties)
        {
            var schemaProperty = new Dictionary<string, object>
            {
                ["type"] = MapJsonTypeToParameterType(property.Type),
            };

            if (!string.IsNullOrWhiteSpace(property.Description))
            {
                schemaProperty["description"] = property.Description;
            }

            if (property.Enum is { Count: > 0 })
            {
                schemaProperty["enum"] = property.Enum
                    .Select(value => value?.ToString() ?? string.Empty)
                    .Where(value => value.Length > 0)
                    .ToArray();
            }

            properties[key] = schemaProperty;
        }

        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
        };

        if (inputSchema.Required is { Count: > 0 })
        {
            schema["required"] = inputSchema.Required;
        }

        return JsonSerializer.Serialize(schema);
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
        var property = new McpToolProperty();


        if(propertyElement.TryGetProperty("anyOf",out var anyOfElement) && anyOfElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in anyOfElement.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var typeElement))
                {
                    property.Type = typeElement.GetString() ?? "string";
                    break;
                }
                else if (item.ValueKind == JsonValueKind.String)
                {
                    property.Type = item.GetString() ?? "string";
                    break;
                }
            }
        }
        else
        {
            property.Type = propertyElement.GetProperty("type").GetString() ?? "string";
        }

        if (propertyElement.TryGetProperty("description", out var descriptionElement))
        {
            property.Description = descriptionElement.GetString();
        }

        if (propertyElement.TryGetProperty("default", out var defaultElement))
        {
            property.Default = GetJsonValue(defaultElement);
        }

        if (propertyElement.TryGetProperty("enum", out var enumElement))
        {
            property.Enum = ParseEnumList(enumElement);
        }
        else if (propertyElement.TryGetProperty("const", out var constElement))
        {
            property.Enum = ParseEnumList(constElement);
        }
        else if (propertyElement.TryGetProperty("anyOf", out var anyOfForEnum)
                 && anyOfForEnum.ValueKind == JsonValueKind.Array)
        {
            foreach (var option in anyOfForEnum.EnumerateArray())
            {
                if (option.TryGetProperty("enum", out var nestedEnum))
                {
                    property.Enum = ParseEnumList(nestedEnum);
                    break;
                }

                if (option.TryGetProperty("const", out var nestedConst))
                {
                    property.Enum = ParseEnumList(nestedConst);
                    break;
                }
            }
        }

        if (propertyElement.TryGetProperty("items", out var itemsElement))
        {
            property.Items = ParseProperty(itemsElement);
        }

        return property;
    }

    private static List<object>? ParseEnumList(JsonElement enumElement) =>
        enumElement.ValueKind switch
        {
            JsonValueKind.Array => enumElement.EnumerateArray().Select(GetJsonValue).ToList(),
            JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False
                => new List<object> { GetJsonValue(enumElement) },
            _ => null,
        };

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
