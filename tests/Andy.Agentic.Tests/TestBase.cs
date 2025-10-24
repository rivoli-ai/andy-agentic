using Andy.Agentic.Domain.Models;
using System.Security.Claims;

namespace Andy.Agentic.Tests;

public abstract class TestBase
{
    protected static ClaimsPrincipal CreateTestUser(string role = "Write", string userId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "test@example.com"),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    protected static Agent CreateTestAgent(Guid? id = null, string name = "Test Agent")
    {
        return new Agent
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = "Test Description",
            Instructions = "Test Instructions",
            Model = "gpt-4",
            Temperature = 0.7f,
            MaxTokens = 1000,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static Tool CreateTestTool(Guid? id = null, string name = "Test Tool", ToolType type = ToolType.API)
    {
        return new Tool
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Type = type,
            Description = "Test Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static LLMConfig CreateTestLlmConfig(Guid? id = null, string name = "Test Config")
    {
        return new LLMConfig
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Provider = LLMProviderType.OpenAi,
            ApiKey = "test-key",
            BaseUrl = "https://api.openai.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static ChatMessage CreateTestChatMessage(Guid? agentId = null, string content = "Hello")
    {
        return new ChatMessage
        {
            AgentId = agentId ?? Guid.NewGuid(),
            Content = content,
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };
    }

    protected static ChatHistoryDto CreateTestChatHistory(string role = "user", string content = "Hello")
    {
        return new ChatHistoryDto
        {
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
    }

    protected static ChatSessionDto CreateTestChatSession(Guid? agentId = null, string agentName = "Test Agent")
    {
        return new ChatSessionDto
        {
            Id = Guid.NewGuid(),
            AgentId = agentId ?? Guid.NewGuid(),
            AgentName = agentName,
            LastMessage = "Hello",
            LastMessageTime = DateTime.UtcNow
        };
    }

    protected static ToolExecutionLogDto CreateTestToolExecutionLog(string toolName = "TestTool", bool success = true)
    {
        return new ToolExecutionLogDto
        {
            Id = Guid.NewGuid(),
            ToolName = toolName,
            Success = success,
            Result = success ? "Tool executed successfully" : null,
            ErrorMessage = success ? null : "Tool execution failed",
            ExecutedAt = DateTime.UtcNow,
            ExecutionTime = 100
        };
    }
}
