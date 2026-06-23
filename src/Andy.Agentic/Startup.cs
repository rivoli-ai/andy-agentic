using System.Diagnostics.CodeAnalysis;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Application.Mapping;
using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Interfaces.Llm;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Infrastructure.Data;
using Andy.Agentic.Infrastructure.Mapping;
using Andy.Agentic.Infrastructure.Repositories;
using Andy.Agentic.Infrastructure.Repositories.Database;
using Andy.Agentic.Infrastructure.Repositories.Llm;
using Andy.Agentic.Infrastructure.Semantic;
using Andy.Agentic.Infrastructure.Semantic.Provider;
using Andy.Agentic.Infrastructure.Services;
using Andy.Agentic.Infrastructure.Services.SkillRegistry;
using Andy.Agentic.Infrastructure.Services.ToolProviders;
using Andy.Agentic.Infrastructure.DependencyInjection;
using Andy.Agentic.Infrastructure.UnitOfWorks;
using Andy.Agentic.Mcps;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;

namespace Andy.Agentic;

/// <summary>
///     Startup configuration class that organizes application configuration into logical sections.
/// </summary>
public class Startup(IConfiguration configuration)
{

    /// <summary>
    ///     Configures the application services.
    /// </summary>
    [Experimental("SKEXP0001")]
    public void ConfigureApplicationServices(IServiceCollection services)
    {
        ConfigureWebServices(services);
        ConfigureDatabase(services);
        ConfigureMapster(services);
        ConfigureRepositories(services);
        ConfigureCoreServices(services);
        ConfigureHttpClient(services);
        ConfigureToolProviders(services);

        // Legacy /sse (+ message endpoint) is required for MCP clients that use the URL from the agent
        // integration snippet (e.g. Claude Desktop). Streamable HTTP stays on / when Stateless is false.
        services.AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = false;
#pragma warning disable MCP9004 // Legacy SSE: opt-in for external MCP clients; prefer streamable HTTP for new clients.
                options.EnableLegacySse = true;
#pragma warning restore MCP9004
            })
            .WithTools<AgentMcp>();

        services.AddSemanticKernelBuilder();
    }

    /// <summary>
    ///     Configures the web application.
    /// </summary>
    public void ConfigureApplication(WebApplication app)
    {
        ConfigureDevelopmentEnvironment(app);
        ConfigureMiddleware(app);
        ConfigureDatabaseInitialization(app);
    }

    /// <summary>
    ///     Configures web-related services like controllers, CORS, authentication, and Swagger.
    /// </summary>
    private void ConfigureWebServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSignalR();

        ConfigureAuthentication(services);
        ConfigureAuthorization(services);
        ConfigureCors(services);
    }

    /// <summary>
    ///     Configures gateway JWT Bearer authentication (issued after OIDC token exchange).
    /// </summary>
    private void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthProviders(configuration);

        var secretKey = configuration["JWT:SecretKey"] ?? "dev-secret-key-min-32-characters-long-for-security";
        var issuer = configuration["JWT:Issuer"] ?? "AndyAgentic";
        var audience = configuration["JWT:Audience"] ?? "AndyAgentic";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                // Keep JWT claim types as-is (sub, email, roles). Default inbound mapping
                // rewrites them to WS-Federation URIs and breaks SyncUser / GetCurrentUser.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    RoleClaimType = ClaimTypes.Role
                };
            });
    }

    /// <summary>
    ///     Configures authorization policies.
    /// </summary>
    private void ConfigureAuthorization(IServiceCollection services) =>
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ReadScope", policy =>
            {
                policy.RequireClaim("scp", "Api.Access");
            });

            options.AddPolicy("WriteRole", policy =>
            {
                policy.RequireRole("Api.Write", "Write");
            });
        });

    /// <summary>
    ///     Configures CORS policy for Angular application.
    /// </summary>
    private void ConfigureCors(IServiceCollection services) =>
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:4200",
                        "http://localhost:4201",
                        "http://127.0.0.1:4200",
                        "http://127.0.0.1:4201")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

    /// <summary>
    ///     Configures database services conditionally based on environment.
    ///     Production/Development: PostgreSQL
    ///     Test: InMemory (configured in test setup)
    /// </summary>
    private void ConfigureDatabase(IServiceCollection services)
    {
        services.AddDbContext<AndyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")
            ));

        services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();
            return dataSourceBuilder.Build();
        });
    }

    /// <summary>
    ///     Configures Mapster (<see href="https://github.com/MapsterMapper/Mapster" />).
    /// </summary>
    private void ConfigureMapster(IServiceCollection services)
    {
        var config = new TypeAdapterConfig();
        new EntityMappingRegister().Register(config);
        new DtosMappingRegister().Register(config);
        services.AddSingleton(config);
        services.AddSingleton<IMapper, ServiceMapper>();
    }


    /// <summary>
    ///     Configures repository registrations.
    /// </summary>
    private void ConfigureRepositories(IServiceCollection services)
    {
        ConfigureDatabaseRepositories(services);
        ConfigureLlmRepositories(services);
        ConfigureUnitOfWork(services);
    }

    /// <summary>
    ///     Configures database repositories.
    /// </summary>
    private void ConfigureDatabaseRepositories(IServiceCollection services)
    {
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<ILlmRepository, LlmRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IToolExecutionRepository, ToolExecutionRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IMcpRepository, McpRepository>();
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IAgentDocumentRepository, AgentDocumentRepository>();
        services.AddScoped<IDocumentExportRepository, DocumentExportRepository>();
        services.AddScoped<ISkillRegistryRepository, SkillRegistryRepository>();
    }

    /// <summary>
    ///     Configures LLM provider repositories.
    /// </summary>
    private void ConfigureLlmRepositories(IServiceCollection services)
    {
        services.AddScoped<ILLmProviderRepository, OpenAiRepository>();
        services.AddScoped<ILLmProviderRepository, OllamaRepository>();
        services.AddScoped<ILlmProviderFactory, LlmProviderFactory>();
    }

    /// <summary>
    ///     Configures Unit of Work pattern.
    /// </summary>
    private void ConfigureUnitOfWork(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }


    /// <summary>
    ///     Configures core application services.
    /// </summary>
    [Experimental("SKEXP0001")]
    private void ConfigureCoreServices(IServiceCollection services)
    {
        ConfigureBusinessServices(services);
        ConfigureDocumentServices(services);
        ConfigureAuthenticationServices(services);
    }

    /// <summary>
    ///     Configures business logic services.
    /// </summary>
    private void ConfigureBusinessServices(IServiceCollection services)
    {
        services.AddScoped<IDataBaseService, DatabaseService>();
        services.AddScoped<ILlmService, LlmService>();
        services.AddScoped<IToolService, ToolService>();
        services.AddScoped<IToolExecutionService, ToolExecutionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<IMcpService, McpService>();
        services.AddScoped<ISkillRegistryClient, SkillRegistryClient>();
        services.AddScoped<ISkillManager, SkillManager>();
    }

    /// <summary>
    ///     Configures document and RAG services.
    /// </summary>
    private void ConfigureDocumentServices(IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentRagProvider, DocumentRagProvider>();

        // Register DocumentRagBackgroundService as singleton and hosted service
        services.AddSingleton<DocumentRagBackgroundService>();
        services.AddSingleton<IDocumentRagBackgroundService>(provider =>
            provider.GetRequiredService<DocumentRagBackgroundService>());
        services.AddHostedService<DocumentRagBackgroundService>(provider =>
            provider.GetRequiredService<DocumentRagBackgroundService>());
    }

    /// <summary>
    ///     Configures authentication services.
    /// </summary>
    private void ConfigureAuthenticationServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddHttpContextAccessor();
    }

    /// <summary>
    ///     Configures HTTP client services.
    /// </summary>
    private void ConfigureHttpClient(IServiceCollection services) => services.AddHttpClient();

    /// <summary>
    ///     Configures tool provider services.
    /// </summary>
    private void ConfigureToolProviders(IServiceCollection services)
    {
        services.AddScoped<IToolProvider, ApiToolProvider>();
        services.AddScoped<IToolProvider, McpToolProvider>();
    }

    /// <summary>
    ///     Configures the development environment settings.
    /// </summary>
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
    private void ConfigureMiddleware(WebApplication app)
    {
        app.UseCors("AllowAngularApp");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<DocumentRagHub>("/documentRagHub");
        app.MapMcp();
    }

    /// <summary>
    ///     Configures database initialization and migration (only for non-test environments).
    /// </summary>
    private void ConfigureDatabaseInitialization(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AndyDbContext>();
        context.Database.Migrate();
    }

}
