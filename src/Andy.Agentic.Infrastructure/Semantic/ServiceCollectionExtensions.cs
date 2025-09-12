using Andy.Agentic.Domain.Interfaces.Llm.Semantic;using Andy.Agentic.Infrastructure.Semantic.Builder;using Andy.Agentic.Infrastructure.Semantic.Provider;using Andy.Agentic.Infrastructure.Semantic.Tools;using Microsoft.Extensions.DependencyInjection;namespace Andy.Agentic.Infrastructure.Semantic;/// <summary>
/// Provides extension methods for configuring services in an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions{
    /// <summary>
    /// Adds the necessary services for the Semantic Kernel Builder to the provided IServiceCollection.
    /// </summary>
    /// <returns>
    /// The updated IServiceCollection with the Semantic Kernel Builder services added.
    /// </returns>
    public static IServiceCollection AddSemanticKernelBuilder(this IServiceCollection services)    {        services.AddSingleton<IProviderDetector, ProviderDetector>();        services.AddSingleton<IAiServiceFactory, AiServiceFactory>();        services.AddSingleton<ISemanticKernelBuilder, SemanticKernelBuilder>();        services.AddSingleton<ApiToolFactory>();        services.AddSingleton<McpToolFactory>();        services.AddSingleton<NativeFunctionToolFactory>();        services.AddSingleton<IToolManager, ToolManager>();        services.AddSingleton<SemanticKernelBuilder>();        services.AddHttpClient();        return services;    }}