using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Application.Mapping;
using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Infrastructure.Data;
using Andy.Agentic.Infrastructure.Mapping;
using Andy.Agentic.Infrastructure.Repositories;
using Andy.Agentic.Infrastructure.Repositories.Database;
using Andy.Agentic.Infrastructure.Repositories.Llm;
using Andy.Agentic.Infrastructure.Services;
using Andy.Agentic.Infrastructure.Services.ToolProviders;
using Andy.Agentic.Infrastructure.UnitOfWorks;
using Andy.ResourceAccess.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Andy.Agentic;

/// <summary>
///     Startup configuration class that organizes application configuration into logical sections.
/// </summary>
public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    ///     Initializes a new instance of the Startup class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The web hosting environment.</param>
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    ///     Configures the application services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public void ConfigureApplicationServices(IServiceCollection services)
    {
        ConfigureWebServices(services);
        ConfigureDatabase(services);
        ConfigureAutoMapper(services);
        ConfigureRepositories(services);
        ConfigureCoreServices(services);
        ConfigureHttpClient(services);
        ConfigureToolProviders(services);
    }

    /// <summary>
    ///     Configures the web application.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    public void ConfigureApplication(WebApplication app)
    {
        ConfigureDevelopmentEnvironment(app);
        ConfigureMiddleware(app);
        ConfigureDatabaseInitialization(app);
    }

    /// <summary>
    ///     Configures web-related services like controllers, CORS, and Swagger.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureWebServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Add JWT Bearer authentication for Microsoft Graph tokens
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = $"{ _configuration["AzureAd:Instance"]}{_configuration["AzureAd:TenantId"]}/v2.0";
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = new[] { $"{_configuration["AzureAd:Audience"]}", $"{_configuration["AzureAd:ClientId"]}" }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ReadScope", policy =>
            {
                policy.RequireClaim("scp", "Api.Access");
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins("http://flexagent.online", "http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // Required for authentication
            });
        });
    }

    /// <summary>
    ///     Configures database-related services and connection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureDatabase(IServiceCollection services) =>
        services.AddDbContext<AndyDbContext>(options =>
            options.UseMySql(
                _configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(_configuration.GetConnectionString("DefaultConnection"))
            ));

    /// <summary>
    ///     Configures AutoMapper profiles.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(EntityMapperProfile));
        services.AddAutoMapper(typeof(DtosMapperProfile));
    }

    /// <summary>
    ///     Configures repository registrations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureRepositories(IServiceCollection services)
    {
        // Database Repositories
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<ILlmRepository, LlmRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IToolExecutionRepository, ToolExecutionRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IMcpRepository, McpRepository>();
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // LLM Provider Repositories
        services.AddScoped<ILLmProviderRepository, OpenAiRepository>();
        services.AddScoped<ILLmProviderRepository, OllamaRepository>();
        services.AddScoped<ILlmProviderFactory, LlmProviderFactory>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    /// <summary>
    ///     Configures core application services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureCoreServices(IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IDataBaseService, DatabaseService>();
        services.AddScoped<ILlmService, LlmService>();
        services.AddScoped<IToolService, ToolService>();
        services.AddScoped<IToolExecutionService, ToolExecutionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAgentService, AgentService>();
        
        // Authentication Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpContextAccessor();
    }

    /// <summary>
    ///     Configures HTTP client services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureHttpClient(IServiceCollection services) => services.AddHttpClient();

    /// <summary>
    ///     Configures tool provider services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureToolProviders(IServiceCollection services)
    {
        services.AddScoped<IToolProvider, ApiToolProvider>();
        services.AddScoped<IToolProvider, McpToolProvider>();
    }

    /// <summary>
    ///     Configures the development environment settings.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    private void ConfigureDevelopmentEnvironment(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }

    /// <summary>
    ///     Configures middleware pipeline.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    private void ConfigureMiddleware(WebApplication app)
    {
        app.UseCors("AllowAngularApp");
        app.UseAuthentication(); // Add this before UseAuthorization
        app.UseAuthorization();
        app.MapControllers();
    }

    /// <summary>
    ///     Configures database initialization and seeding.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    private void ConfigureDatabaseInitialization(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AndyDbContext>();
        context.Database.Migrate();
        //DatabaseSeeder.SeedData(context);
    }
}
