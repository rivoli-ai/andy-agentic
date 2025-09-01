using Andy.Agentic.Domain.Interfaces.Llm;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Factory class for creating and managing LLM provider repositories.
/// Uses dependency injection to resolve the appropriate LLM provider repository
/// based on the requested provider name, providing a centralized way to access
/// different LLM services like OpenAI, Ollama, etc.
/// </summary>
public class LlmProviderFactory(IServiceProvider serviceProvider) : ILlmProviderFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IEnumerable<ILLmProviderRepository> _providers = serviceProvider.GetServices<ILLmProviderRepository>();

    /// <summary>
    /// Gets the appropriate LLM provider repository for the specified provider name.
    /// </summary>
    /// <param name="provider">The name of the LLM provider (e.g., "openai", "ollama").</param>
    /// <returns>The LLM provider repository that can handle the specified provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no repository is found for the specified provider.</exception>
    public ILLmProviderRepository GetProvider(string provider)
    {
        var repository = _providers.FirstOrDefault(p => p.CanHandleProvider(provider));
        
        if (repository == null)
        {
            var availableProviders = string.Join(", ", GetAvailableProviders());
            throw new InvalidOperationException(
                $"No LLM provider repository found for provider '{provider}'. Available providers: {availableProviders}");
        }

        return repository;
    }

    /// <summary>
    /// Gets a list of all available LLM provider names that can be used with this factory.
    /// </summary>
    /// <returns>A collection of provider names that are currently registered and available.</returns>
    public IEnumerable<string> GetAvailableProviders()
    {
        return _providers.Select(p => p.ProviderName);
    }
}

