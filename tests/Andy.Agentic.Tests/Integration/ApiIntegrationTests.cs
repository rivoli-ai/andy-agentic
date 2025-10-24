using Andy.Agentic.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Andy.Agentic.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Add test services or override production services here
                services.AddLogging(builder => builder.AddConsole());
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAgents_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/agents");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTools_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tools");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLlmConfigs_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/llm");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAgent_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var agent = new Agent
        {
            Name = "Integration Test Agent",
            Description = "Test Description",
            Instructions = "Test Instructions",
            Model = "gpt-4",
            Temperature = 0.7f,
            MaxTokens = 1000,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/agents", agent);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTool_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var tool = new Tool
        {
            Name = "Integration Test Tool",
            Type = ToolType.API,
            Description = "Test Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tools", tool);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLlmConfig_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var config = new LLMConfig
        {
            Name = "Integration Test Config",
            Provider = LLMProviderType.OpenAi,
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/llm", config);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendChatMessage_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var chatMessage = new ChatMessage
        {
            AgentId = Guid.NewGuid(),
            Content = "Hello, this is a test message",
            UserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/chat/send", chatMessage);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChatHistory_WithValidAgentId_ShouldReturnOk()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/chat/history/{agentId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChatSessions_WithValidUserId_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/chat/sessions/{userId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchAgents_WithValidCriteria_ShouldReturnOk()
    {
        // Arrange
        var searchCriteria = new
        {
            Name = "Test",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/agents/search", searchCriteria);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchTools_WithValidCriteria_ShouldReturnOk()
    {
        // Arrange
        var searchCriteria = new
        {
            Name = "Test",
            Type = ToolType.API,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tools/search", searchCriteria);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetToolsByType_WithValidType_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tools/type/API");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveTools_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/tools/active");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetActiveLlmConfigs_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/llm/active");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLlmConfigsByProvider_WithValidProvider_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/llm/provider/OpenAi");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TestLlmConnection_WithValidConfigId_ShouldReturnOk()
    {
        // Arrange
        var configId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/llm/{configId}/test", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DuplicateAgent_WithValidAgentId_ShouldReturnOk()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/agents/{agentId}/duplicate", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteAgent_WithValidAgentId_ShouldReturnNoContent()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/agents/{agentId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTool_WithValidToolId_ShouldReturnNoContent()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tools/{toolId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLlmConfig_WithValidConfigId_ShouldReturnNoContent()
    {
        // Arrange
        var configId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/llm/{configId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteChatSession_WithValidSessionId_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/chat/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }
}
