using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Moq;
using FluentAssertions;
using MapsterMapper;

namespace Andy.Agentic.Tests.Controllers;

public class ToolsControllerTests
{
    private readonly Mock<IToolService> _mockToolService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IMcpService> _mockMcpService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly ToolsController _controller;

    public ToolsControllerTests()
    {
        _mockToolService = new Mock<IToolService>();
        _mockMapper = new Mock<IMapper>();
        _mockMcpService = new Mock<IMcpService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(new UserDto { Id = _currentUserId, Email = "test@example.com", DisplayName = "Test" });
        _controller = new ToolsController(_mockToolService.Object, _mockMapper.Object, _mockMcpService.Object, _mockAuthService.Object);
        
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
        var mockedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Tool 1", Type = "api", Description = "Description 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Tool 2", Type = "mcp", Description = "Description 2", IsActive = true }
        };

        _mockToolService
            .Setup(x => x.GetAllToolsAsync())
            .ReturnsAsync(mockedTools);


        // Act
        var result = await _controller.GetTools();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<ToolDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTools);
        _mockToolService.Verify(x => x.GetAllToolsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetToolById_WithValidId_ShouldReturnTool()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var mockedTool = new Tool
        {
            Id = toolId,
            Name = "Test Tool",
            Type = "api",
            Description = "Test Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        _mockToolService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync(mockedTool);

        // Act
        var result = await _controller.GetTool(toolId);

        // Assert
        result.Should().BeOfType<ActionResult<ToolDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTool);
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
        var result = await _controller.GetTool(toolId);

        // Assert
        result.Should().BeOfType<ActionResult<ToolDto>>()
            .Which.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockToolService.Verify(x => x.GetToolByIdAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task CreateTool_WithValidTool_ShouldReturnCreatedTool()
    {
        // Arrange
        var createToolDto = new ToolDto
        {
            Name = "New Tool",
            Type = "api",
            Description = "New Description",
            Configuration = "{\"url\": \"https://api.example.com\"}",
            IsActive = true
        };

        var mockedTool = new Tool
        {
            Id = Guid.NewGuid(),
            Name = createToolDto.Name,
            Type = createToolDto.Type,
            Description = createToolDto.Description,
            Configuration = createToolDto.Configuration,
            IsActive = createToolDto.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<Tool>(createToolDto))
            .Returns(mockedTool);

        _mockToolService
            .Setup(x => x.CreateToolAsync(It.IsAny<Tool>()))
            .ReturnsAsync(mockedTool);


        // Act
        var result = await _controller.CreateTool(createToolDto);

        // Assert
        mockedTool.CreatedByUserId.Should().Be(_currentUserId);
        result.Should().BeOfType<ActionResult<ToolDto>>()
            .Which.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTool);
        _mockToolService.Verify(x => x.CreateToolAsync(It.IsAny<Tool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTool_WithValidTool_ShouldReturnUpdatedTool()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var updateToolDto = new ToolDto
        {
            Id = toolId,
            Name = "Updated Tool",
            Type = "mcp",
            Description = "Updated Description",
            Configuration = "{\"url\": \"https://updated-api.example.com\"}",
            IsActive = true
        };

        var existingOwnerId = Guid.NewGuid();
        _mockToolService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync(new Tool { Id = toolId, CreatedByUserId = existingOwnerId, Name = "Old" });

        var mockedTool = new Tool
        {
            Id = updateToolDto.Id!.Value,
            Name = updateToolDto.Name,
            Type = updateToolDto.Type,
            Description = updateToolDto.Description,
            Configuration = updateToolDto.Configuration,
            IsActive = updateToolDto.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<Tool>(updateToolDto))
            .Returns(mockedTool);

        _mockToolService
            .Setup(x => x.UpdateToolAsync(It.IsAny<Guid>(), It.IsAny<Tool>()))
            .ReturnsAsync((Guid _, Tool t) => t);

        // Act
        var result = await _controller.UpdateTool(updateToolDto.Id!.Value, updateToolDto);

        // Assert
        mockedTool.Id.Should().Be(toolId);
        mockedTool.CreatedByUserId.Should().Be(existingOwnerId);
        result.Should().BeOfType<ActionResult<ToolDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTool);
        _mockToolService.Verify(x => x.UpdateToolAsync(It.IsAny<Guid>(), It.IsAny<Tool>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTool_WithMismatchedId_ShouldReturnOk()
    {
        // Arrange
        var toolId = Guid.NewGuid();
        var updateToolDto = new ToolDto
        {
            Id = Guid.NewGuid(), // Different ID - controller doesn't validate this
            Name = "Updated Tool",
            Type = "api",
            Description = "Updated Description"
        };

        var existingOwnerId = Guid.NewGuid();
        _mockToolService
            .Setup(x => x.GetToolByIdAsync(toolId))
            .ReturnsAsync(new Tool { Id = toolId, CreatedByUserId = existingOwnerId });

        var mockedTool = new Tool
        {
            Id = toolId,
            Name = "Updated Tool",
            Type = "api",
            Description = "Updated Description"
        };

        _mockMapper
            .Setup(x => x.Map<Tool>(updateToolDto))
            .Returns(mockedTool);

        _mockToolService
            .Setup(x => x.UpdateToolAsync(toolId, It.IsAny<Tool>()))
            .ReturnsAsync((Guid _, Tool t) => t);

        // Act
        var result = await _controller.UpdateTool(toolId, updateToolDto);

        // Assert
        mockedTool.CreatedByUserId.Should().Be(existingOwnerId);
        result.Should().BeOfType<ActionResult<ToolDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTool);
        _mockToolService.Verify(x => x.UpdateToolAsync(toolId, It.IsAny<Tool>()), Times.Once);
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
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockToolService.Verify(x => x.DeleteToolAsync(toolId), Times.Once);
    }

    [Fact]
    public async Task SearchTools_WithValidCriteria_ShouldReturnMatchingTools()
    {
        // Arrange
        var searchQuery = "Test";

        var mockedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Tool 1", Type = "api", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Test Tool 2", Type = "api", IsActive = true }
        };

        _mockToolService
            .Setup(x => x.SearchToolsAsync(searchQuery))
            .ReturnsAsync(mockedTools);

        // Act
        var result = await _controller.SearchTools(searchQuery);

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<ToolDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTools);
        _mockToolService.Verify(x => x.SearchToolsAsync(searchQuery), Times.Once);
    }

    [Fact]
    public async Task GetToolsByType_WithValidType_ShouldReturnToolsOfType()
    {
        // Arrange
        var toolType = "api";
        var mockedTools = new List<Tool>
        {
            new() { Id = Guid.NewGuid(), Name = "API Tool 1", Type = "api" },
            new() { Id = Guid.NewGuid(), Name = "API Tool 2", Type = "api" }
        };

        _mockToolService
            .Setup(x => x.GetToolsByTypeAsync(toolType))
            .ReturnsAsync(mockedTools);


        // Act
        var result = await _controller.GetToolsByType(toolType);

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<ToolDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedTools);
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
        result.Should().BeOfType<ActionResult<IEnumerable<ToolDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedTools);
        _mockToolService.Verify(x => x.GetActiveToolsAsync(), Times.Once);
    }
}

