using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools;
using Microsoft.SemanticKernel;
using System.Text.Json;
using Moq;
using FluentAssertions;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Tools;

public class ApiToolFactoryTests
{
    private readonly ApiToolFactory _apiToolFactory;

    public ApiToolFactoryTests()
    {
        _apiToolFactory = new ApiToolFactory();
    }

    [Fact]
    public void CreateTool_WithValidApiTool_ShouldReturnTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "TestApiTool",
            Type = "api",
            Description = "Test API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                endpoint = "https://api.example.com/test",
                method = "GET"
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Authorization", value = "Bearer token123" },
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
        result.Description.Should().Be(tool.Description);
    }

    [Fact]
    public void CreateTool_WithPostMethod_ShouldCreatePostTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "PostApiTool",
            Type = "api",
            Description = "POST API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                endpoint = "https://api.example.com/create",
                method = "POST"
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithPutMethod_ShouldCreatePutTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "PutApiTool",
            Type = "api",
            Description = "PUT API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                endpoint = "https://api.example.com/update/123",
                method = "PUT",
                body = new { name = "updated", value = 456 }
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithDeleteMethod_ShouldCreateDeleteTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "DeleteApiTool",
            Type = "api",
            Description = "DELETE API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/delete/123",
                method = "DELETE",
                headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token123" }
                }
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithPatchMethod_ShouldCreatePatchTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "PatchApiTool",
            Type = "api",
            Description = "PATCH API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/patch/123",
                method = "PATCH",
                body = new { name = "patched" }
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithHeadMethod_ShouldCreateHeadTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "HeadApiTool",
            Type = "api",
            Description = "HEAD API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/head",
                method = "HEAD",
                headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token123" }
                }
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithOptionsMethod_ShouldCreateOptionsTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "OptionsApiTool",
            Type = "api",
            Description = "OPTIONS API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/options",
                method = "OPTIONS",
                headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token123" }
                }
            }),
            Headers = JsonSerializer.Serialize(new[]
            {
                new { name = "Content-Type", value = "application/json" }
            }),
            IsActive = true
        };

        // Act
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Agent Description",
            IsActive = true
        };

        var result = _apiToolFactory.CreateToolAsync(agent, tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

   
}

