using Andy.Agentic.Domain.Models;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;/// <summary>
/// Interface for a factory that creates tool instances.
/// </summary>
public interface IToolFactory{
    /// <summary>
    /// Asynchronously creates a tool based on the provided configuration.
    /// </summary>
    /// <param name="config">The configuration settings for the tool.</param>
    /// <returns>A task representing the asynchronous operation, containing the created kernel function.</returns>
    KernelFunction CreateToolAsync(Tool config);}