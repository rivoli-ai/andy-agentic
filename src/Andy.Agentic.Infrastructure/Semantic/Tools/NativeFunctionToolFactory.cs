using System.Diagnostics.CodeAnalysis;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Infrastructure.Semantic.Tools;

public class NativeFunctionToolFactory : IToolFactory
{
    private readonly ILogger<NativeFunctionToolFactory>? _logger;
    private readonly DocumentRagServiceTool _documentRagServiceTool;
    private readonly DocumentExportTool _documentExportTool;

    public NativeFunctionToolFactory(
        DocumentRagServiceTool documentRagServiceTool,
        DocumentExportTool documentExportTool,
        ILogger<NativeFunctionToolFactory>? logger = null)
    {
        _documentRagServiceTool = documentRagServiceTool;
        _documentExportTool = documentExportTool;
        _logger = logger;
    }
    [Experimental("SKEXP0130")]
    public KernelFunction CreateToolAsync(Agent agent, Tool tool)
    {
        try
        {
            _logger?.LogInformation("Creating native function tool: {ToolName}", tool.Name);

            return tool.Name.ToLowerInvariant() switch
            {
                "search" => CreateSearchTool(agent, tool),
                "export" => CreateExportTool(agent, tool),
                _ => CreateGenericNativeTool(tool)
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating native function tool: {ToolName}", tool.Name);
            throw;
        }
    }

    /// <summary>
    /// Creates a search tool function using DocumentRagServiceTool.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="config">The tool configuration.</param>
    /// <returns>A KernelFunction for document search.</returns>
    [Experimental("SKEXP0130")]
    private KernelFunction CreateSearchTool(Agent agent, Tool config)
    {
        _logger?.LogInformation("Creating Search tool with DocumentRagService functionality");

        return KernelFunctionFactory.CreateFromMethod(
            async (string query) => 
                await _documentRagServiceTool.SearchDocumentsAsync(query, agent),
            functionName: config.Name,
            description: config.Description
        );
    }

    /// <summary>
    /// Creates an export tool function using DocumentExportTool.
    /// </summary>
    /// <param name="agent">The agent executing the tool.</param>
    /// <param name="config">The tool configuration.</param>
    /// <returns>A KernelFunction for document export.</returns>
    [Experimental("SKEXP0130")]
    private KernelFunction CreateExportTool(Agent agent, Tool config)
    {
        _logger?.LogInformation("Creating Export tool with DocumentExportTool functionality");

        // Pass the tool configuration and agent to access apiUrl from config and agentId
        // Note: sessionId is not available in the function signature, so we pass null
        // It could be obtained from a context service in the future if needed
        return KernelFunctionFactory.CreateFromMethod(
            async (string content, string format, string? title) =>
                await _documentExportTool.ExportDocumentAsync(content, format, title, config, agent, null),
            functionName: config.Name,
            description: config.Description
        );
    }

    /// <summary>
    /// Creates a generic native tool function for unknown tool names.
    /// </summary>
    /// <param name="config">The tool configuration.</param>
    /// <returns>A KernelFunction representing a generic native tool.</returns>
    private KernelFunction CreateGenericNativeTool(Tool config)
    {
        _logger?.LogInformation("Creating generic native tool: {ToolName}", config.Name);

        // For now, return a simple function that indicates the tool is not implemented
        return KernelFunctionFactory.CreateFromMethod(
            method: () => Task.FromResult($"Native tool '{config.Name}' is not yet implemented."),
            functionName: config.Name,
            description: config.Description
        );
    }
}
