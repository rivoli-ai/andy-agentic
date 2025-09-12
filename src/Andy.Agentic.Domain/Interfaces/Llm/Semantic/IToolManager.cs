using Andy.Agentic.Domain.Models;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;/// <summary>
/// Interface for managing tools within the application.
/// Provides methods for adding, removing, and retrieving tools.
/// </summary>
public interface IToolManager{
    /// <summary>
    /// Asynchronously adds a list of tool configurations to the specified kernel.
    /// </summary>
    /// <param name="kernel">The kernel to which the tools will be added.</param>
    /// <param name="tools">A list of tool configurations to be added to the kernel.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    void AddToolsAsync(Kernel kernel, List<Tool> tools);}