using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Andy.Agentic.Application.Tests.Services;

/// <summary>Unit tests for <see cref="SkillManager"/>.</summary>
public class SkillManagerTests
{
    private readonly Mock<ISkillRegistryRepository> _repo = new();
    private readonly Mock<ISkillRegistryClient> _client = new();
    private readonly SkillManager _sut;

    /// <summary>Initializes the system under test.</summary>
    public SkillManagerTests()
    {
        _sut = new SkillManager(_repo.Object, _client.Object);
    }

    /// <summary>CreateRegistry persists and returns the created connection.</summary>
    [Fact]
    public async Task CreateRegistryAsync_PersistsAndReturns()
    {
        _repo.Setup(r => r.CreateAsync(It.IsAny<SkillRegistryEntity>()))
            .ReturnsAsync((SkillRegistryEntity e) => { e.Id = Guid.NewGuid(); return e; });

        var result = await _sut.CreateRegistryAsync(new SkillRegistry { Name = "Hub", BaseUrl = "http://x" });

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Hub");
        _repo.Verify(r => r.CreateAsync(It.IsAny<SkillRegistryEntity>()), Times.Once);
    }

    /// <summary>TestRegistry loads the connection and delegates to the client.</summary>
    [Fact]
    public async Task TestRegistryAsync_DelegatesToClient()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new SkillRegistryEntity { Id = id, BaseUrl = "http://x" });
        _client.Setup(c => c.TestConnectionAsync(It.IsAny<SkillRegistry>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        (await _sut.TestRegistryAsync(id)).Should().BeTrue();
    }

    /// <summary>TestRegistry throws when the connection does not exist.</summary>
    [Fact]
    public async Task TestRegistryAsync_WhenMissing_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SkillRegistryEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.TestRegistryAsync(Guid.NewGuid()));
    }

    /// <summary>AttachSkill resolves the registry and persists the association under the agent.</summary>
    [Fact]
    public async Task AttachSkillAsync_PersistsUnderAgent()
    {
        var agentId = Guid.NewGuid();
        var registryId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(registryId)).ReturnsAsync(new SkillRegistryEntity { Id = registryId, BaseUrl = "http://x" });
        _repo.Setup(r => r.AttachSkillAsync(It.IsAny<AgentSkillEntity>()))
            .ReturnsAsync((AgentSkillEntity e) => { e.Id = Guid.NewGuid(); return e; });

        var result = await _sut.AttachSkillAsync(agentId, new AgentSkill
        {
            SkillRegistryId = registryId,
            Namespace = "acme",
            SkillSlug = "pdf-filler",
            Version = "1.0.0",
            DisplayName = "PDF Filler",
        });

        result.AgentId.Should().Be(agentId);
        result.SkillSlug.Should().Be("pdf-filler");
        _repo.Verify(r => r.AttachSkillAsync(It.Is<AgentSkillEntity>(e => e.AgentId == agentId && e.SkillRegistryId == registryId)), Times.Once);
    }

    /// <summary>DetachSkill delegates to the repository.</summary>
    [Fact]
    public async Task DetachSkillAsync_DelegatesToRepository()
    {
        var agentId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        _repo.Setup(r => r.DetachSkillAsync(agentId, skillId)).ReturnsAsync(true);

        (await _sut.DetachSkillAsync(agentId, skillId)).Should().BeTrue();
    }
}
