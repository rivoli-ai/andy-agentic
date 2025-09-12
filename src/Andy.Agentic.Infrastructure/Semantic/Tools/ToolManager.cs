using Andy.Agentic.Domain.Interfaces.Llm.Semantic;using Andy.Agentic.Domain.Models;using Andy.Agentic.Domain.Models.Semantic;using Microsoft.Extensions.Logging;using Microsoft.SemanticKernel;namespace Andy.Agentic.Infrastructure.Semantic.Tools;/// <summary>
/// Represents a manager responsible for handling tools.
/// Implements the IToolManager interface.
/// </summary>
public class ToolManager : IToolManager{    private readonly Dictionary<ToolType, IToolFactory> _toolFactories;    private readonly ILogger<ToolManager>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolManager"/> class.
    /// </summary>
    /// <param name="apiToolFactory">Factory for creating API tools.</param>
    /// <param name="mcpToolFactory">Factory for creating MCP tools.</param>
    /// <param name="nativeToolFactory">Factory for creating native function tools.</param>
    /// <param name="logger">Optional logger for logging operations.</param>
    public ToolManager(
            ApiToolFactory apiToolFactory,
            McpToolFactory mcpToolFactory,
            NativeFunctionToolFactory nativeToolFactory,
            ILogger<ToolManager>? logger = null)    {        _toolFactories = new Dictionary<ToolType, IToolFactory>        {            { ToolType.ApiTool, apiToolFactory },            { ToolType.McpTool, mcpToolFactory },            { ToolType.NativeFunction, nativeToolFactory }        };        _logger = logger;    }

    /// <summary>
    /// Adds a list of tools to the specified kernel asynchronously.
    /// </summary>
    /// <param name="kernel">The kernel to which the tools will be added.</param>
    /// <param name="tools">A list of tool configurations to be added.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public void AddToolsAsync(Kernel kernel, List<Tool> tools)    {        foreach (var tool in tools)        {            try            {                if (_toolFactories.TryGetValue(Enum.Parse<ToolType>(tool.Type), out var factory))                {                    var function = factory.CreateToolAsync(tool);                    kernel.ImportPluginFromFunctions(tool.Name, [function]);                    _logger?.LogInformation("Successfully added tool: {ToolName} of type {ToolType}", tool.Name, tool.Type);                }                else                {                    _logger?.LogWarning("Unknown tool type: {ToolType} for tool: {ToolName}", tool.Type, tool.Name);                }            }            catch (Exception ex)            {                _logger?.LogError(ex, "Failed to add tool: {ToolName}", tool.Name);            }        }    }}