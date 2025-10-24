using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Tests.Services;

public class SimpleToolServiceTests
{
    private readonly Mock<IDataBaseService> _mockDatabaseService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ToolService _toolService;

    public SimpleToolServiceTests()
    {
        _mockDatabaseService = new Mock<IDataBaseService>();
        _mockMapper = new Mock<IMapper>();
        _toolService = new ToolService(_mockDatabaseService.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllToolsAsync_ShouldReturnAllTools()
    {
        // Arrange
        var expectedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Tool 1", Type = "api", Description = "Description 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Tool 2", Type = "mcp", Description = "Description 2", IsActive = true }
        };

        _mockDatabaseService
            .Setup(x => x.GetAllToolsAsync())
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _toolService.GetAllToolsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedTools);
        _mockDatabaseService.Verify(x => x.GetAllToolsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetToolByIdAsync_WithValidId_ShouldReturnTool()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var expectedTool = new Tool
        {
            Id = toolId,
            Name = "Test Tool",
            Type = "api",
            Description = "Test Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        _mockDatabaseService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync(expectedTool);

        // Act
        var result = await _toolService.GetToolByIdAsync(toolId);

        // Assert
        result.Should().BeEquivalentTo(expectedTool);
        _mockDatabaseService.Verify(x => x.GetToolByIdAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task GetToolByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync((Tool?)null);

        // Act
        var result = await _toolService.GetToolByIdAsync(toolId);

        // Assert
        result.Should().BeNull();
        _mockDatabaseService.Verify(x => x.GetToolByIdAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task CreateToolAsync_WithValidTool_ShouldReturnCreatedTool()
    {
        // Arrange
        var createTool = new Tool
        {
            Name = "New Tool",
            Type = "api",
            Description = "New Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        var expectedTool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = createTool.Name,
            Type = createTool.Type,
            Description = createTool.Description,
            Configuration = createTool.Configuration,
            IsActive = createTool.IsActive
        };

        _mockDatabaseService
            .Setup(x => x.CreateToolAsync(createTool))
            .ReturnsAsync(expectedTool);

        // Act
        var result = await _toolService.CreateToolAsync(createTool);

        // Assert
        result.Should().BeEquivalentTo(expectedTool);
        _mockDatabaseService.Verify(x => x.CreateToolAsync(createTool), Times.Once);
    }

    [Fact]
    public async Task UpdateToolAsync_WithValidTool_ShouldReturnUpdatedTool()
    {
        // Arrange
        var updateTool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "Updated Tool",
            Type = "mcp",
            Description = "Updated Description",
            Configuration = "{\"url\": \"https://updated-api.example.com\"}",
            IsActive = true
        };

        _mockDatabaseService
            .Setup(x => x.UpdateToolAsync(updateTool.Id, updateTool))
            .ReturnsAsync(updateTool);

        // Act
        var result = await _toolService.UpdateToolAsync(updateTool.Id, updateTool);

        // Assert
        result.Should().BeEquivalentTo(updateTool);
        _mockDatabaseService.Verify(x => x.UpdateToolAsync(updateTool.Id, updateTool), Times.Once);
    }

    [Fact]
    public async Task DeleteToolAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.DeleteToolAsync(toolId))
            .ReturnsAsync(true);

        // Act
        var result = await _toolService.DeleteToolAsync(toolId);

        // Assert
        result.Should().BeTrue();
        _mockDatabaseService.Verify(x => x.DeleteToolAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task DeleteToolAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.DeleteToolAsync(toolId))
            .ReturnsAsync(false);

        // Act
        var result = await _toolService.DeleteToolAsync(toolId);

        // Assert
        result.Should().BeFalse();
        _mockDatabaseService.Verify(x => x.DeleteToolAsync(toolId), Times.Once);
    }
}
