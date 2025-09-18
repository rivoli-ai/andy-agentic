using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Infrastructure.Semantic.Builder;
using Andy.Agentic.Infrastructure.Semantic.Provider;
using Andy.Agentic.Infrastructure.Semantic.Tools;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.Agentic.Infrastructure.Semantic;

/// <summary>
/// Provides extension methods for configuring services in an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the necessary services for the Semantic Kernel Builder to the provided IServiceCollection.
    /// </summary>
    /// <returns>
    /// The updated IServiceCollection with the Semantic Kernel Builder services added.
    /// </returns>
    public static IServiceCollection AddSemanticKernelBuilder(this IServiceCollection services)
    {
        services.AddScoped<IProviderDetector, ProviderDetector>();
        services.AddScoped<IAiServiceFactory, AiServiceFactory>();
        services.AddScoped<ISemanticKernelBuilder, SemanticKernelBuilder>();
        services.AddScoped<ApiToolFactory>();
        services.AddScoped<McpToolFactory>();
        services.AddScoped<DocumentRagServiceTool>();
        services.AddScoped<NativeFunctionToolFactory>();
        services.AddScoped<IToolManager, ToolManager>();
        services.AddScoped<SemanticKernelBuilder>();
        services.AddHttpClient();

        return services;
    }
}
