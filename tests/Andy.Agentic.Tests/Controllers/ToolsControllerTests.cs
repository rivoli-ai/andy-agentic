using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Andy.Agentic.Tests.Controllers;

public class ToolsControllerTests
{
    private readonly Mock<IToolService> _mockToolService;
    private readonly Mock<ILogger<ToolsController>> _mockLogger;
    private readonly ToolsController _controller;

    public ToolsControllerTests()
    {
        _mockToolService = new Mock<IToolService>();
        _mockLogger = new Mock<ILogger<ToolsController>>();
        _controller = new ToolsController(_mockToolService.Object, _mockLogger.Object);
        
        // Setup controller context with user claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "test@example.com"),
            new(ClaimTypes.Role, "Write")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task GetAllTools_ShouldReturnAllTools()
    {
        // Arrange
        var expectedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Tool 1", Type = ToolType.API, Description = "Description 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Tool 2", Type = ToolType.MCP, Description = "Description 2", IsActive = true }
        };

        _mockToolService
            .Setup(x => x.GetAllToolsAsync())
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _controller.GetAllTools();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTools);
        _mockToolService.Verify(x => x.GetAllToolsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetToolById_WithValidId_ShouldReturnTool()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var expectedTool = new Tool
        {
            Id = toolId,
            Name = "Test Tool",
            Type = ToolType.API,
            Description = "Test Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        _mockToolService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync(expectedTool);

        // Act
        var result = await _controller.GetToolById(toolId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTool);
        _mockToolService.Verify(x => x.GetToolByIdAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task GetToolById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockToolService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync((Tool?)null);

        // Act
        var result = await _controller.GetToolById(toolId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockToolService.Verify(x => x.GetToolByIdAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task CreateTool_WithValidTool_ShouldReturnCreatedTool()
    {
        // Arrange
        var createTool = new Tool
        {
            Name = "New Tool",
            Type = ToolType.API,
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

        _mockToolService
            .Setup(x => x.CreateToolAsync(createTool))
            .ReturnsAsync(expectedTool);

        // Act
        var result = await _controller.CreateTool(createTool);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedTool);
        _mockToolService.Verify(x => x.CreateToolAsync(createTool), Times.Once);
    }

    [Fact]
    public async Task UpdateTool_WithValidTool_ShouldReturnUpdatedTool()
    {
        // Arrange
        var updateTool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "Updated Tool",
            Type = ToolType.MCP,
            Description = "Updated Description",
            Configuration = "{\"url\": \"https://updated-api.example.com\"}",
            IsActive = true
        };

        _mockToolService
            .Setup(x => x.UpdateToolAsync(updateTool))
            .ReturnsAsync(updateTool);

        // Act
        var result = await _controller.UpdateTool(updateTool.Id, updateTool);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updateTool);
        _mockToolService.Verify(x => x.UpdateToolAsync(updateTool), Times.Once);
    }

    [Fact]
    public async Task UpdateTool_WithMismatchedId_ShouldReturnBadRequest()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var updateTool = new Tool
        {
            Id = Guid.NewGuid(), // Different ID
            Name = "Updated Tool",
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.UpdateTool(toolId, updateTool);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockToolService.Verify(x => x.UpdateToolAsync(It.IsAny<Tool>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTool_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockToolService
            .Setup(x => x.DeleteToolAsync(toolId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTool(toolId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockToolService.Verify(x => x.DeleteToolAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task DeleteTool_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var toolId = Guid.NewGuid();

        _mockToolService
            .Setup(x => x.DeleteToolAsync(toolId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTool(toolId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockToolService.Verify(x => x.DeleteToolAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task SearchTools_WithValidCriteria_ShouldReturnMatchingTools()
    {
        // Arrange
        var searchCriteria = new ToolSearchCriteria
        {
            Name = "Test",
            Type = ToolType.API,
            IsActive = true
        };

        var expectedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Tool 1", Type = ToolType.API, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Test Tool 2", Type = ToolType.API, IsActive = true }
        };

        _mockToolService
            .Setup(x => x.SearchToolsAsync(searchCriteria))
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _controller.SearchTools(searchCriteria);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTools);
        _mockToolService.Verify(x => x.SearchToolsAsync(searchCriteria), Times.Once);
    }

    [Fact]
    public async Task GetToolsByType_WithValidType_ShouldReturnToolsOfType()
    {
        // Arrange
        var toolType = ToolType.API;
        var expectedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "API Tool 1", Type = ToolType.API },
            new() { Id = Guid.NewGuid(), Name = "API Tool 2", Type = ToolType.API }
        };

        _mockToolService
            .Setup(x => x.GetToolsByTypeAsync(toolType))
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _controller.GetToolsByType(toolType);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTools);
        _mockToolService.Verify(x => x.GetToolsByTypeAsync(toolType), Times.Once);
    }

    [Fact]
    public async Task GetActiveTools_ShouldReturnOnlyActiveTools()
    {
        // Arrange
        var expectedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Active Tool 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Active Tool 2", IsActive = true }
        };

        _mockToolService
            .Setup(x => x.GetActiveToolsAsync())
            .ReturnsAsync(expectedTools);

        // Act
        var result = await _controller.GetActiveTools();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTools);
        _mockToolService.Verify(x => x.GetActiveToolsAsync(), Times.Once);
    }
}
