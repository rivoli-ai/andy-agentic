using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;/// <summary>
/// Defines a factory interface for creating instances of AI services.
/// </summary>
public interface IAiServiceFactory{
    /// <summary>
    /// Adds an AI service to the specified kernel builder with the given configuration and provider.
    /// </summary>
    /// <param name="builder">The kernel builder to which the AI service will be added.</param>
    /// <param name="config">The configuration settings for the AI service.</param>
    /// <param name="provider">The provider of the AI service.</param>
    void AddAiService(IKernelBuilder builder, LlmConfig config, LLMProviderType provider);}