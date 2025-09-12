namespace Andy.Agentic.Domain.Models.Semantic;/// <summary>
/// Represents the configuration settings for a tool.
/// </summary>
public class ToolConfiguration{
    /// <summary>
    /// Gets or sets the name. Defaults to an empty string.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the type of the tool.
    /// </summary>
    public ToolType Type { get; set; }
    /// <summary>
    /// Gets or sets the endpoint URL. This property can be null.
    /// </summary>
    public string? Endpoint { get; set; }
    /// <summary>
    /// Gets or sets the HTTP method to be used for the request.
    /// </summary>
    public string? HttpMethod { get; set; }
    /// <summary>
    /// Gets or sets the headers as a dictionary where the key is the header name and the value is the header value.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
    /// <summary>
    /// Gets or sets the content template. This property can be null.
    /// </summary>
    public string? ContentTemplate { get; set; }
    /// <summary>
    /// Gets or sets the query parameter. This can be null.
    /// </summary>
    public string? QueryParameter { get; set; }
    /// <summary>
    /// Gets or sets the parameters as a dictionary where the key is a string and the value is an object.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
    /// <summary>
    /// Gets or sets the path to the MCP server. This property can be null.
    /// </summary>
    public string? McpServerPath { get; set; }
    /// <summary>
    /// Gets or sets the assembly name for the native function.
    /// </summary>
    public string? NativeFunctionAssembly { get; set; }
    /// <summary>
    /// Gets or sets the type of the native function. This property can be null.
    /// </summary>
    public string? NativeFunctionType { get; set; }
    /// <summary>
    /// Gets or sets the native function method. This property can hold a null value.
    /// </summary>
    public string? NativeFunctionMethod { get; set; }}