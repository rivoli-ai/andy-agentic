using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models;

/// <summary>
///     Represents a tool call issued by an LLM, including the function and arguments.
/// </summary>
public class ToolCall
{
    /// <summary>
    ///     Unique identifier for the tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Tool call type (usually "function").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    ///     Function call details including name and raw argument JSON.
    /// </summary>
    [JsonPropertyName("function")]
    public ToolCallFunction Function { get; set; } = new();
}
