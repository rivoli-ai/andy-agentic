using Andy.Agentic.Domain.Interfaces.Llm.Semantic;using Andy.Agentic.Domain.Models;using Andy.Agentic.Domain.Models.Semantic;using Microsoft.Extensions.Logging;using Microsoft.SemanticKernel;namespace Andy.Agentic.Infrastructure.Semantic.Provider;/// <summary>
/// Factory class responsible for creating instances of AI services.
/// Implements the IAIServiceFactory interface.
/// </summary>
public class AiServiceFactory : IAiServiceFactory{    private readonly ILogger<AiServiceFactory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiServiceFactory"/> class.
    /// </summary>
    /// <param name="logger">An optional logger instance for logging purposes.</param>
    public AiServiceFactory(ILogger<AiServiceFactory>? logger = null)    {        _logger = logger;    }

    /// <summary>
    /// Adds the AI service to the kernel builder based on the specified provider.
    /// </summary>
    /// <param name="builder">The kernel builder to which the AI service will be added.</param>
    /// <param name="config">The configuration settings for the AI service.</param>
    /// <param name="provider">The AI provider to be used.</param>
    /// <exception cref="NotSupportedException">Thrown when the specified provider is not supported.</exception>
    public void AddAiService(IKernelBuilder builder, LlmConfig config, LLMProviderType provider)    {        switch (provider)        {            case LLMProviderType.OpenAi:                AddOpenAiService(builder, config);                break;            case LLMProviderType.AzureOpenAi:                AddAzureOpenAiService(builder, config);                break;            default:                throw new NotSupportedException($"Provider {provider} is not supported");        }    }

    /// <summary>
    /// Adds the OpenAI service to the kernel builder with the specified configuration.
    /// </summary>
    /// <param name="builder">The kernel builder to which the OpenAI service will be added.</param>
    /// <param name="config">The configuration containing the API key, model ID, and base URL for the OpenAI service.</param>
    private void AddOpenAiService(IKernelBuilder builder, LlmConfig config)    {        var apiKey = config.ApiKey ?? throw new ArgumentException("OpenAI API Key is required");        var modelId = config.Model;        var baseUrl = config.BaseUrl;        builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey, endpoint: new Uri(baseUrl));        _logger?.LogInformation("Added OpenAI service with model: {ModelId}", modelId);    }

    /// <summary>
    /// Adds the Azure OpenAI service to the kernel builder with the specified configuration.
    /// </summary>
    /// <param name="builder">The kernel builder to which the Azure OpenAI service will be added.</param>
    /// <param name="config">The configuration containing the necessary details for the Azure OpenAI service.</param>
    /// <exception cref="ArgumentException">Thrown when any of the required configuration parameters are null or empty.</exception>
    private void AddAzureOpenAiService(IKernelBuilder builder, LlmConfig config)    {        var endpoint = config.BaseUrl ?? throw new ArgumentException("Azure endpoint is required");        var apiKey = config.ApiKey ?? throw new ArgumentException("Azure API Key is required");        var deploymentName = config.Model ?? throw new ArgumentException("Azure deployment name is required");        builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);        _logger?.LogInformation("Added Azure OpenAI service with deployment: {DeploymentName}", deploymentName);    }}