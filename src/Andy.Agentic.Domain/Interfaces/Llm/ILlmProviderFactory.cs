namespace Andy.Agentic.Domain.Interfaces.Llm;

/// <summary>
///     Factory for obtaining provider-specific LLM repositories.
/// </summary>
public interface ILlmProviderFactory
{
    /// <summary>
    ///     Gets the appropriate LLM provider repository for the given provider name
    /// </summary>
    /// <param name="provider">Provider name (e.g., "openai", "ollama")</param>
    /// <returns>LLM provider repository</returns>
    ILLmProviderRepository GetProvider(string provider);

    /// <summary>
    ///     Gets all available provider names
    /// </summary>
    /// <returns>List of available provider names</returns>
    IEnumerable<string> GetAvailableProviders();
}
