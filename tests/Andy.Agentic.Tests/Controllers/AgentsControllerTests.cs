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
using Andy.Agentic.Application.DTOs;

namespace Andy.Agentic.Tests.Controllers;

public class AgentsControllerTests
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AgentsController _controller;

    public AgentsControllerTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockMapper = new Mock<IMapper>();
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AgentsController(_mockAgentService.Object, _mockMapper.Object, _mockAuthService.Object);

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
        var mockedAgents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Agent 1", Description = "Description 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Agent 2", Description = "Description 2", IsActive = true }
        };

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockAgentService
            .Setup(x => x.GetVisibleAgentsAsync(currentUser.Id))
            .ReturnsAsync(mockedAgents);

        // Act
        var result = await _controller.GetAgents();

        // Assert
        result.Should().BeOfType<ActionResult<IEnumerable<AgentDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedAgents);
        _mockAgentService.Verify(x => x.GetVisibleAgentsAsync(currentUser.Id), Times.Once);
    }

    [Fact]
    public async Task GetAgentById_WithValidId_ShouldReturnAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var mockedAgent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Description = "Test Description",
            IsActive = true
        };


        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockAgentService
            .Setup(x => x.GetVisibleAgentByIdAsync(agentId, currentUser.Id))
            .ReturnsAsync(mockedAgent);


        // Act
        var result = await _controller.GetAgent(agentId);

        // Assert
        result.Should().BeOfType<ActionResult<AgentDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedAgent);
        _mockAgentService.Verify(x => x.GetVisibleAgentByIdAsync(agentId, currentUser.Id), Times.Once);
    }

    [Fact]
    public async Task GetAgentById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        _mockAgentService
            .Setup(x => x.GetVisibleAgentByIdAsync(agentId, currentUser.Id))
            .ReturnsAsync((Agent?)null);

        // Act
        var result = await _controller.GetAgent(agentId);

        // Assert
        result.Should().BeOfType<ActionResult<AgentDto>>()
            .Which.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockAgentService.Verify(x => x.GetVisibleAgentByIdAsync(agentId, currentUser.Id), Times.Once);
    }

    /// <summary>
    /// CreateAgent_WithValidAgent_ShouldReturnCreatedAgent
    /// </summary>
    [Fact]
    public async Task CreateAgent_WithValidAgent_ShouldReturnCreatedAgent()
    {
        var createAgent = new AgentDto
        {
            Name = "New Agent",
            Description = "New Description",
            IsActive = true
        };

        var expectedAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = createAgent.Name,
            Description = createAgent.Description,
            IsActive = createAgent.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<Agent>(createAgent))
            .Returns(expectedAgent);

        _mockAgentService
            .Setup(x => x.CreateAgentAsync(It.IsAny<Agent>()))
            .ReturnsAsync(expectedAgent);

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        var result = await _controller.CreateAgent(createAgent);

        result.Result.Should().BeOfType<CreatedAtActionResult>();

        ((CreatedAtActionResult)result.Result!).Value.Should().BeEquivalentTo(expectedAgent);


        _mockAgentService.Verify(x => x.CreateAgentAsync(It.IsAny<Agent>()), Times.Once);
    }

    /// <summary>
    /// UpdateAgent_WithValidAgent_ShouldReturnUpdatedAgent
    /// </summary>
    [Fact]
    public async Task UpdateAgent_WithValidAgent_ShouldReturnUpdatedAgent()
    {
        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);



        // Arrange
        var updateAgent = new AgentDto
        {
            CreatedByUserId = currentUser.Id,
            Id = Guid.NewGuid(),
            Name = "Updated Agent",
            Description = "Updated Description",
            IsActive = true
        };

        var mockedAgent = new Agent
        {
            Id = updateAgent.Id!.Value,
            Name = updateAgent.Name,
            Description = updateAgent.Description,
            IsActive = updateAgent.IsActive,
            CreatedByUserId = currentUser.Id
        };

        _mockAgentService
            .Setup(x => x.GetAgentByIdAsync(updateAgent.Id!.Value))
            .ReturnsAsync(mockedAgent);

        _mockMapper
            .Setup(x => x.Map<Agent>(updateAgent))
            .Returns(mockedAgent);

        _mockAgentService
            .Setup(x => x.UpdateAgentAsync(It.IsAny<Agent>()))
            .ReturnsAsync(mockedAgent);

        _mockMapper
            .Setup(x => x.Map<AgentDto>(mockedAgent))
            .Returns(updateAgent);

        var result = await _controller.UpdateAgent(updateAgent.Id!.Value, updateAgent);

        // Assert
        result.Should().BeOfType<ActionResult<AgentDto>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(mockedAgent);

        _mockAgentService.Verify(x => x.UpdateAgentAsync(It.IsAny<Agent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAgent_WithMismatchedId_ShouldReturnBadRequest()
    {
        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };

        _mockAuthService
            .Setup(x => x.GetCurrentUserAsync())
            .ReturnsAsync(currentUser);

        // Arrange
        var agentId = Guid.NewGuid();
        var updateAgent = new AgentDto
        {
            CreatedByUserId = currentUser.Id,
            Id = Guid.NewGuid(), // Different ID
            Name = "Updated Agent",
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.UpdateAgent(agentId, updateAgent);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        _mockAgentService.Verify(x => x.UpdateAgentAsync(It.IsAny<Agent>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAgent_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };
        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(currentUser);


        _mockAgentService
            .Setup(x => x.GetAgentByIdAsync(agentId))
            .ReturnsAsync(new Agent { Id = agentId, CreatedByUserId = currentUser.Id });

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

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };
        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(currentUser);

        _mockAgentService
            .Setup(x => x.DeleteAgentAsync(agentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockAgentService.Verify(x => x.DeleteAgentAsync(agentId), Times.Never);
    }

    [Fact]
    public async Task SearchAgents_WithValidCriteria_ShouldReturnMatchingAgents()
    {
        // Arrange
        var searchCriteria = "Test";

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
        result.Should().BeOfType<ActionResult<IEnumerable<AgentDto>>>()
            .Which.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedAgents);
        _mockAgentService.Verify(x => x.SearchAgentsAsync(searchCriteria), Times.Once);
    }

    [Fact]
    public async Task DuplicateAgent_WithValidId_ShouldReturnDuplicatedAgent()
    {
        // Arrange
        var duplicatedAgentDto = new AgentDto
        {
            Id = Guid.NewGuid(),
            Name = "Original Agent (Copy)",
            Description = "Original Description",
            IsActive = true
        };

        var createdAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = duplicatedAgentDto.Name,
            Description = duplicatedAgentDto.Description,
            IsActive = duplicatedAgentDto.IsActive
        };

        _mockMapper
            .Setup(x => x.Map<Agent>(duplicatedAgentDto))
            .Returns(createdAgent);

        _mockAgentService
            .Setup(x => x.CreateAgentAsync(It.IsAny<Agent>()))
            .ReturnsAsync(createdAgent);

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };
        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(currentUser);

        // Act
        var result = await _controller.CreateAgent(duplicatedAgentDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result.Result!).Value.Should().BeEquivalentTo(createdAgent);
        _mockAgentService.Verify(x => x.CreateAgentAsync(It.IsAny<Agent>()), Times.Once);
    }

    [Fact]
    public async Task DuplicateAgent_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var duplicatedAgentDto = new AgentDto
        {
            Id = Guid.NewGuid()
        };

        var agentToCreate = new Agent { Id = duplicatedAgentDto.Id!.Value };

        _mockMapper
            .Setup(x => x.Map<Agent>(duplicatedAgentDto))
            .Returns(agentToCreate);

        _mockAgentService
            .Setup(x => x.CreateAgentAsync(It.IsAny<Agent>()))
            .ThrowsAsync(new ArgumentException("Agent not found"));

        var currentUser = new UserDto { Id = Guid.NewGuid(), Email = "test@example.com" };
        _mockAuthService.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(currentUser);

        // Act
        var result = await _controller.CreateAgent(duplicatedAgentDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _mockAgentService.Verify(x => x.CreateAgentAsync(It.IsAny<Agent>()), Times.Once);
    }
}
