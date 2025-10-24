using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Andy.Agentic.Infrastructure.Tests.Tools;

public class ApiToolFactoryTests
{
    private readonly Mock<ILogger<ApiToolFactory>> _mockLogger;
    private readonly ApiToolFactory _apiToolFactory;

    public ApiToolFactoryTests()
    {
        _mockLogger = new Mock<ILogger<ApiToolFactory>>();
        _apiToolFactory = new ApiToolFactory(_mockLogger.Object);
    }

    [Fact]
    public void CreateTool_WithValidApiTool_ShouldReturnTool()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "TestApiTool",
            Type = ToolType.API,
            Description = "Test API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/test",
                method = "GET",
                headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token123" },
                    { "Content-Type", "application/json" }
                }
            }),
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
            Description = "POST API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/create",
                method = "POST",
                headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                body = new { name = "test", value = 123 }
            }),
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
            Description = "PUT API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/update/123",
                method = "PUT",
                headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                body = new { name = "updated", value = 456 }
            }),
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
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
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
            Description = "PATCH API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/patch/123",
                method = "PATCH",
                headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                body = new { name = "patched" }
            }),
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
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
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

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
            Type = ToolType.API,
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
            IsActive = true
        };

        // Act
        var result = _apiToolFactory.CreateTool(tool);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tool.Name);
    }

    [Fact]
    public void CreateTool_WithInvalidConfiguration_ShouldThrowException()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "InvalidApiTool",
            Type = ToolType.API,
            Description = "Invalid API Tool",
            Configuration = "invalid json",
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<JsonException>(() => _apiToolFactory.CreateTool(tool));
    }

    [Fact]
    public void CreateTool_WithMissingUrl_ShouldThrowException()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "MissingUrlTool",
            Type = ToolType.API,
            Description = "Missing URL Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                method = "GET"
                // Missing url
            }),
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _apiToolFactory.CreateTool(tool));
    }

    [Fact]
    public void CreateTool_WithEmptyConfiguration_ShouldThrowException()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "EmptyConfigTool",
            Type = ToolType.API,
            Description = "Empty Config Tool",
            Configuration = "",
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _apiToolFactory.CreateTool(tool));
    }

    [Fact]
    public void CreateTool_WithNullConfiguration_ShouldThrowException()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "NullConfigTool",
            Type = ToolType.API,
            Description = "Null Config Tool",
            Configuration = null,
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _apiToolFactory.CreateTool(tool));
    }

    [Fact]
    public void CreateTool_WithNonApiTool_ShouldThrowException()
    {
        // Arrange
        var tool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "NonApiTool",
            Type = ToolType.MCP, // Not API type
            Description = "Non API Tool",
            Configuration = JsonSerializer.Serialize(new
            {
                url = "https://api.example.com/test",
                method = "GET"
            }),
            IsActive = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _apiToolFactory.CreateTool(tool));
    }
}
