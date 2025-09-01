using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Serialization;

namespace Andy.Agentic.Domain.Models;


/// <summary>
/// class representing a tool compatible with OpenAI's function calling feature.
/// </summary>
public class OpenAiTool
{
    /// <summary>
    /// Gets or sets the type of the object. Defaults to "function".
    /// </summary>

    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// gets or sets the function details, including its name, description, and parameters.
    /// </summary>

    [JsonPropertyName("function")]
    public Function Function { get; set; } = new();
}

/// <summary>
///  class representing the details of a function that can be called by OpenAI's models.
/// </summary>
public class Function
{
    /// <summary>
    /// gets or sets the name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// gets or sets the description of the function.
    /// </summary>

    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// gets or sets the parameters of the function, including their types and descriptions.
    /// </summary>

    [JsonPropertyName("parameters")]
    public FunctionParameters Parameters { get; set; } = new();
}

/// <summary>
///  class representing the parameters of a function, including their types and descriptions.
/// </summary>
public class FunctionParameters
{
    /// <summary>
    /// gets or sets the type of the parameters object. Always "object".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// gets or sets the individual properties of the parameters, each defined by a name and its corresponding FunctionProperty details.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, FunctionProperty> Properties { get; set; } = new();

    /// <summary>
    /// gets or sets the list of required parameter names.
    /// </summary>

    [JsonPropertyName("required")]
    public string[] Required { get; set; } = Array.Empty<string>();
}

/// <summary>
/// class representing a single property of a function's parameters, including its type, description, and possible enum values.
/// </summary>
public class FunctionProperty
{

    /// <summary>
    /// gets or sets the type of the property. Can be "string", "number", "boolean", "array", or "object". Defaults to "string".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    /// <summary>
    /// gets or sets the description of the property.
    /// </summary>

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// gets or sets the enumeration of possible values for the property, if applicable.
    /// </summary>
    [JsonPropertyName("enum")]
    public string[]? Enum { get; set; }

    /// <summary>
    /// gets or sets the items definition if the property type is "array".
    /// </summary>
    [JsonPropertyName("items")]
    public FunctionProperty? Items { get; set; }
}
