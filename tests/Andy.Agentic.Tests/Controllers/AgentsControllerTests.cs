using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Controllers;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Andy.Agentic.Tests.Controllers;

public class AgentsControllerTests
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<ILogger<AgentsController>> _mockLogger;
    private readonly AgentsController _controller;

    public AgentsControllerTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockLogger = new Mock<ILogger<AgentsController>>();
        _controller = new AgentsController(_mockAgentService.Object, _mockLogger.Object);
        
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
    public async Task GetAllAgents_ShouldReturnAllAgents()
    {
        // Arrange
        var expectedAgents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Agent 1", Description = "Description 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Agent 2", Description = "Description 2", IsActive = true }
        };

        _mockAgentService
            .Setup(x => x.GetAllAgentsAsync())
            .ReturnsAsync(expectedAgents);

        // Act
        var result = await _controller.GetAllAgents();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAgents);
        _mockAgentService.Verify(x => x.GetAllAgentsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAgentById_WithValidId_ShouldReturnAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedAgent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Description = "Test Description",
            Instructions = "Test Instructions",
            Model = "gpt-4",
            Temperature = 0.7f,
            MaxTokens = 1000,
            IsActive = true
        };

        _mockAgentService
            .Setup(x => x.GetAgentByIdAsync(agentId))
            .ReturnsAsync(expectedAgent);

        // Act
        var result = await _controller.GetAgentById(agentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAgent);
        _mockAgentService.Verify(x => x.GetAgentByIdAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task GetAgentById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService
            .Setup(x => x.GetAgentByIdAsync(agentId))
            .ReturnsAsync((Agent?)null);

        // Act
        var result = await _controller.GetAgentById(agentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockAgentService.Verify(x => x.GetAgentByIdAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task CreateAgent_WithValidAgent_ShouldReturnCreatedAgent()
    {
        // Arrange
        var createAgent = new Agent
        {
            Name = "New Agent",
            Description = "New Description",
            Instructions = "New Instructions",
            Model = "gpt-4",
            Temperature = 0.7f,
            MaxTokens = 1000,
            IsActive = true
        };

        var expectedAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = createAgent.Name,
            Description = createAgent.Description,
            Instructions = createAgent.Instructions,
            Model = createAgent.Model,
            Temperature = createAgent.Temperature,
            MaxTokens = createAgent.MaxTokens,
            IsActive = createAgent.IsActive
        };

        _mockAgentService
            .Setup(x => x.CreateAgentAsync(createAgent))
            .ReturnsAsync(expectedAgent);

        // Act
        var result = await _controller.CreateAgent(createAgent);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedAgent);
        _mockAgentService.Verify(x => x.CreateAgentAsync(createAgent), Times.Once);
    }

    [Fact]
    public async Task UpdateAgent_WithValidAgent_ShouldReturnUpdatedAgent()
    {
        // Arrange
        var updateAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Updated Agent",
            Description = "Updated Description",
            Instructions = "Updated Instructions",
            Model = "gpt-4",
            Temperature = 0.8f,
            MaxTokens = 1500,
            IsActive = true
        };

        _mockAgentService
            .Setup(x => x.UpdateAgentAsync(updateAgent))
            .ReturnsAsync(updateAgent);

        // Act
        var result = await _controller.UpdateAgent(updateAgent.Id, updateAgent);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updateAgent);
        _mockAgentService.Verify(x => x.UpdateAgentAsync(updateAgent), Times.Once);
    }

    [Fact]
    public async Task UpdateAgent_WithMismatchedId_ShouldReturnBadRequest()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var updateAgent = new Agent
        {
            Id = Guid.NewGuid(), // Different ID
            Name = "Updated Agent",
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.UpdateAgent(agentId, updateAgent);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockAgentService.Verify(x => x.UpdateAgentAsync(It.IsAny<Agent>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAgent_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService
            .Setup(x => x.DeleteAgentAsync(agentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockAgentService.Verify(x => x.DeleteAgentAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAgent_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService
            .Setup(x => x.DeleteAgentAsync(agentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockAgentService.Verify(x => x.DeleteAgentAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task SearchAgents_WithValidCriteria_ShouldReturnMatchingAgents()
    {
        // Arrange
        var searchCriteria = new AgentSearchCriteria
        {
            Name = "Test",
            IsActive = true
        };

        var expectedAgents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Agent 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Test Agent 2", IsActive = true }
        };

        _mockAgentService
            .Setup(x => x.SearchAgentsAsync(searchCriteria))
            .ReturnsAsync(expectedAgents);

        // Act
        var result = await _controller.SearchAgents(searchCriteria);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAgents);
        _mockAgentService.Verify(x => x.SearchAgentsAsync(searchCriteria), Times.Once);
    }

    [Fact]
    public async Task DuplicateAgent_WithValidId_ShouldReturnDuplicatedAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var duplicatedAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Original Agent (Copy)",
            Description = "Original Description",
            Instructions = "Original Instructions",
            Model = "gpt-4",
            Temperature = 0.7f,
            MaxTokens = 1000,
            IsActive = true
        };

        _mockAgentService
            .Setup(x => x.DuplicateAgentAsync(agentId))
            .ReturnsAsync(duplicatedAgent);

        // Act
        var result = await _controller.DuplicateAgent(agentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(duplicatedAgent);
        _mockAgentService.Verify(x => x.DuplicateAgentAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task DuplicateAgent_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService
            .Setup(x => x.DuplicateAgentAsync(agentId))
            .ThrowsAsync(new ArgumentException("Agent not found"));

        // Act
        var result = await _controller.DuplicateAgent(agentId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Agent not found");
        _mockAgentService.Verify(x => x.DuplicateAgentAsync(agentId), Times.Once);
    }
}
