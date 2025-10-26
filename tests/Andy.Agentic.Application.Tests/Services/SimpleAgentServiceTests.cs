using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Tests.Services;

public class SimpleAgentServiceTests
{
    private readonly Mock<IDataBaseService> _mockDatabaseService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly AgentService _agentService;

    public SimpleAgentServiceTests()
    {
        _mockDatabaseService = new Mock<IDataBaseService>();
        _mockMapper = new Mock<IMapper>();
        _agentService = new AgentService(_mockDatabaseService.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllAgentsAsync_ShouldReturnAllAgents()
    {
        // Arrange
        var expectedAgents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Agent 1", Description = "Description 1", Type = "ChatBot", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Agent 2", Description = "Description 2", Type = "Assistant", IsActive = true }
        };

        _mockDatabaseService
            .Setup(x => x.GetAllAgentsAsync())
            .ReturnsAsync(expectedAgents);

        // Act
        var result = await _agentService.GetAllAgentsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedAgents);
        _mockDatabaseService.Verify(x => x.GetAllAgentsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAgentByIdAsync_WithValidId_ShouldReturnAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedAgent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Description = "Test Description",
            Type = "ChatBot",
            IsActive = true,
            LlmConfigId = Guid.NewGuid()
        };

        _mockDatabaseService
            .Setup(x => x.GetAgentByIdAsync(agentId))
            .ReturnsAsync(expectedAgent);

        // Act
        var result = await _agentService.GetAgentByIdAsync(agentId);

        // Assert
        result.Should().BeEquivalentTo(expectedAgent);
        _mockDatabaseService.Verify(x => x.GetAgentByIdAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task GetAgentByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.GetAgentByIdAsync(agentId))
            .ReturnsAsync((Agent?)null);

        // Act
        var result = await _agentService.GetAgentByIdAsync(agentId);

        // Assert
        result.Should().BeNull();
        _mockDatabaseService.Verify(x => x.GetAgentByIdAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task CreateAgentAsync_WithValidAgent_ShouldReturnCreatedAgent()
    {
        // Arrange
        var createAgent = new Agent
        {
            Name = "New Agent",
            Description = "New Description",
            Type = "ChatBot",
            IsActive = true,
            LlmConfigId = Guid.NewGuid()
        };

        var mappedAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = createAgent.Name,
            Description = createAgent.Description,
            Type = createAgent.Type,
            IsActive = createAgent.IsActive,
            LlmConfigId = createAgent.LlmConfigId
        };

        var expectedAgent = new Agent
        {
            Id = mappedAgent.Id,
            Name = mappedAgent.Name,
            Description = mappedAgent.Description,
            Type = mappedAgent.Type,
            IsActive = mappedAgent.IsActive,
            LlmConfigId = mappedAgent.LlmConfigId
        };

        _mockMapper
            .Setup(x => x.Map<Agent>(createAgent))
            .Returns(mappedAgent);

        _mockDatabaseService
            .Setup(x => x.CreateAgentAsync(mappedAgent))
            .ReturnsAsync(expectedAgent);

        // Act
        var result = await _agentService.CreateAgentAsync(createAgent);

        // Assert
        result.Should().BeEquivalentTo(expectedAgent);
        _mockMapper.Verify(x => x.Map<Agent>(createAgent), Times.Once);
        _mockDatabaseService.Verify(x => x.CreateAgentAsync(mappedAgent), Times.Once);
    }

    [Fact]
    public async Task UpdateAgentAsync_WithValidAgent_ShouldReturnUpdatedAgent()
    {
        // Arrange
        var updateAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Updated Agent",
            Description = "Updated Description",
            Type = "Assistant",
            IsActive = true,
            LlmConfigId = Guid.NewGuid()
        };

        _mockDatabaseService
            .Setup(x => x.UpdateAgentAsync(updateAgent))
            .ReturnsAsync(updateAgent);

        // Act
        var result = await _agentService.UpdateAgentAsync(updateAgent);

        // Assert
        result.Should().BeEquivalentTo(updateAgent);
        _mockDatabaseService.Verify(x => x.UpdateAgentAsync(updateAgent), Times.Once);
    }

    [Fact]
    public async Task DeleteAgentAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.DeleteAgentAsync(agentId))
            .ReturnsAsync(true);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId);

        // Assert
        result.Should().BeTrue();
        _mockDatabaseService.Verify(x => x.DeleteAgentAsync(agentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAgentAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockDatabaseService
            .Setup(x => x.DeleteAgentAsync(agentId))
            .ReturnsAsync(false);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId);

        // Assert
        result.Should().BeFalse();
        _mockDatabaseService.Verify(x => x.DeleteAgentAsync(agentId), Times.Once);
    }
}


