using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;/// <summary>
/// Interface for detecting and providing information about various providers.
/// </summary>
public interface IProviderDetector{
    /// <summary>
    /// Detects and returns the appropriate AI provider based on the given kernel configuration.
    /// </summary>
    /// <param name="config">The kernel configuration used to determine the AI provider.</param>
    /// <returns>The detected AI provider.</returns>
    LLMProviderType DetectProvider(LlmConfig config);}