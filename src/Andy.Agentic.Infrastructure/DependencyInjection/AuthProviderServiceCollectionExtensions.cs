using Andy.Agentic.Application.Auth;
using Andy.Agentic.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.Agentic.Infrastructure.DependencyInjection;

public static class AuthProviderServiceCollectionExtensions
{
    /// <summary>Binds <c>AuthProviders</c> and registers OIDC validation + gateway JWT minting for SPA login.</summary>
    public static IServiceCollection AddAuthProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthProvidersOptions>(options =>
        {
            var section = configuration.GetSection(AuthProvidersOptions.SectionName);
            var providers = section.Get<Dictionary<string, ProviderConfig>>() ?? new Dictionary<string, ProviderConfig>();
            options.Providers = new Dictionary<string, ProviderConfig>(providers, StringComparer.OrdinalIgnoreCase);
        });
        services.AddSingleton<AuthProviderRegistry>();
        services.AddSingleton<GatewayAuthenticationService>();
        services.AddHttpClient();
        return services;
    }
}
