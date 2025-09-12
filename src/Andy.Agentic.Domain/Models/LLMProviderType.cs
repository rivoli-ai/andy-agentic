namespace Andy.Agentic.Domain.Models;

/// <summary>
/// Enumeration representing different LLM providers.
/// </summary>
public enum LLMProviderType
{
    /// <summary>
    /// OpenAI provider
    /// </summary>
    OpenAi = 0,

    /// <summary>
    /// Anthropic provider
    /// </summary>
    Anthropic = 1,

    /// <summary>
    /// Google provider
    /// </summary>
    Google = 2,

    /// <summary>
    /// Custom provider
    /// </summary>
    Custom = 3,

    /// <summary>
    /// Ollama provider
    /// </summary>
    Ollama = 4,

    /// <summary>
    /// Azure OpenAI provider
    /// </summary>
    AzureOpenAi = 5,
}
