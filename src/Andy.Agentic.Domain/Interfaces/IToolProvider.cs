using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
/// Interface for tool providers that can execute different types of tools
/// </summary>
public interface IToolProvider
{
    /// <summary>
    /// Gets the tool type this provider can handle
    /// </summary>
    string ToolType { get; }

    /// <summary>
    /// Executes a tool with the given parameters
    /// </summary>
    /// <param name="tool">The tool configuration</param>
    /// <param name="requestParameters"></param>
    /// <param name="parameters">The parameters to pass to the tool</param>
    /// <returns>The result of the tool execution</returns>
    Task<object?> ExecuteToolAsync(Tool tool, Dictionary<string, object> requestParameters);

    /// <summary>
    /// Checks if this provider can handle the given tool type
    /// </summary>
    /// <param name="toolType">The tool type to check</param>
    /// <returns>True if this provider can handle the tool type</returns>
    bool CanHandleToolType(string toolType);
}
