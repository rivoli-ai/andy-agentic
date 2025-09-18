namespace Andy.Agentic.Domain.Models.Semantic;

/// <summary>
/// Represents the different types of tools available in the system.
/// </summary>
public enum ToolType
{
    /// <summary>
    /// ApiTool Enum
    /// </summary>
    ApiTool,

    /// <summary>
    /// McpTool Enum
    /// </summary>
    McpTool,

    /// <summary>
    /// InternalTool Enum - Simple internal tools with no parameters
    /// </summary>
    InternalTool
}
