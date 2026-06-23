using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Microsoft.Extensions.Logging;namespace Andy.Agentic.Infrastructure.Semantic.Provider;/// <summary>
/// Represents a class that detects providers, optionally using a logger for logging purposes.
/// </summary>
public class ProviderDetector(ILogger<ProviderDetector>? logger = null) : IProviderDetector{
    /// <summary>
    /// Detects the AI provider based on the given configuration.
    /// If the provider is explicitly set in the configuration, it uses that.
    /// Otherwise, it defaults to AiProvider.None.
    /// </summary>
    /// <param name="config">The configuration containing the provider information.</param>
    /// <returns>
    /// The detected AI provider based on the configuration.
    /// </returns>
    public LLMProviderType DetectProvider(LlmConfig config)    {
        return config.Provider;    }}