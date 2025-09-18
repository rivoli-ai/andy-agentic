using System.ComponentModel.DataAnnotations;

namespace Andy.Agentic.Domain.Models;

/// <summary>
/// Represents an MCP tool discovered from an MCP server.
/// </summary>
public class McpToolDiscovery
{
    /// <summary>
    /// Gets or sets the name of the tool.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the tool.
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input schema for the tool.
    /// </summary>
    public McpToolInputSchema? InputSchema { get; set; }
}

/// <summary>
/// Represents the input schema for an MCP tool.
/// </summary>
public class McpToolInputSchema
{
    /// <summary>
    /// Gets or sets the type of the schema (usually "object").
    /// </summary>
    public string Type { get; set; } = "object";

    /// <summary>
    /// Gets or sets the properties of the input schema.
    /// </summary>
    public Dictionary<string, McpToolProperty>? Properties { get; set; }

    /// <summary>
    /// Gets or sets the required properties.
    /// </summary>
    public List<string>? Required { get; set; }
}

/// <summary>
/// Represents a property in the MCP tool input schema.
/// </summary>
public class McpToolProperty
{
    /// <summary>
    /// Gets or sets the type of the property.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the property.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default value of the property.
    /// </summary>
    public object? Default { get; set; }

    /// <summary>
    /// Gets or sets the enum values if the property is an enum.
    /// </summary>
    public List<object>? Enum { get; set; }

    /// <summary>
    /// Gets or sets the items schema for array types.
    /// </summary>
    public McpToolProperty? Items { get; set; }
}

/// <summary>
/// Represents the response from MCP server tool discovery.
/// </summary>
public class McpToolDiscoveryResponse
{
    /// <summary>
    /// Gets or sets the list of discovered tools.
    /// </summary>
    public List<McpToolDiscovery> Tools { get; set; } = new();

    /// <summary>
    /// Gets or sets any error message if discovery failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets whether the discovery was successful.
    /// </summary>
    public bool Success { get; set; }
}
