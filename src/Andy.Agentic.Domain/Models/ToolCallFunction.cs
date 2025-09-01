using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Details of a function invocation in a tool call.
/// </summary>
public class ToolCallFunction
{
    /// <summary>
    ///     Name of the function being called.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Raw JSON string containing the function arguments.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}
