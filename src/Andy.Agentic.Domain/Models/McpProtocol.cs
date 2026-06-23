using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models;

/// <summary>
/// MCP (Model Context Protocol) message types
/// </summary>
public static class McpMessageTypes
{
    public const string Initialize = "initialize";
    public const string Initialized = "initialized";
    public const string Request = "request";
    public const string Response = "response";
    public const string Notification = "notification";
    public const string Error = "error";
}

/// <summary>
/// Base MCP message
/// </summary>
public class McpMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public object? Params { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public McpError? Error { get; set; }
}

/// <summary>
/// MCP error object
/// </summary>
public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// MCP initialize request parameters
/// </summary>
public class McpInitializeParams
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public McpClientCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("clientInfo")]
    public McpClientInfo ClientInfo { get; set; } = new();
}

/// <summary>
/// MCP client capabilities
/// </summary>
public class McpClientCapabilities
{
    [JsonPropertyName("roots")]
    public McpRootsCapability? Roots { get; set; }

    [JsonPropertyName("sampling")]
    public McpSamplingCapability? Sampling { get; set; }
}

/// <summary>
/// MCP roots capability
/// </summary>
public class McpRootsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// MCP sampling capability
/// </summary>
public class McpSamplingCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// MCP client info
/// </summary>
public class McpClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Andy Agentic";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// MCP initialize result
/// </summary>
public class McpInitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public McpServerCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}

/// <summary>
/// MCP server capabilities
/// </summary>
public class McpServerCapabilities
{
    [JsonPropertyName("roots")]
    public McpRootsCapability? Roots { get; set; }

    [JsonPropertyName("sampling")]
    public McpSamplingCapability? Sampling { get; set; }

    [JsonPropertyName("logging")]
    public McpLoggingCapability? Logging { get; set; }

    [JsonPropertyName("prompts")]
    public McpPromptsCapability? Prompts { get; set; }

    [JsonPropertyName("resources")]
    public McpResourcesCapability? Resources { get; set; }

    [JsonPropertyName("tools")]
    public McpToolsCapability? Tools { get; set; }
}

/// <summary>
/// MCP logging capability
/// </summary>
public class McpLoggingCapability
{
    [JsonPropertyName("level")]
    public string Level { get; set; } = "info";
}

/// <summary>
/// MCP prompts capability
/// </summary>
public class McpPromptsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// MCP resources capability
/// </summary>
public class McpResourcesCapability
{
    [JsonPropertyName("subscribe")]
    public bool Subscribe { get; set; }

    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// MCP tools capability
/// </summary>
public class McpToolsCapability
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; set; }
}

/// <summary>
/// MCP server info
/// </summary>
public class McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// MCP tool definition
/// </summary>
public class McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new();
}

/// <summary>
/// MCP tool call request
/// </summary>
public class McpToolCallRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public Dictionary<string, object> Arguments { get; set; } = new();
}

/// <summary>
/// MCP tool call result
/// </summary>
public class McpToolCallResult
{
    [JsonPropertyName("content")]
    public List<McpContent> Content { get; set; } = new();

    [JsonPropertyName("isError")]
    public bool IsError { get; set; }
}

/// <summary>
/// MCP content
/// </summary>
public class McpContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
