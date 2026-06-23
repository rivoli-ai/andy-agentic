using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal;
using FluentAssertions;
using Moq;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Semantic;

/// <summary>Unit tests for <see cref="SkillToolset"/> progressive-disclosure behavior.</summary>
public class SkillToolsetTests
{
    private static AgentSkill MakeSkill(string display, string slug = "skill", string ns = "acme") => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = display,
        SkillSlug = slug,
        Namespace = ns,
        Version = "1.0.0",
        Description = "does things",
        IsActive = true,
        Registry = new SkillRegistry { Id = Guid.NewGuid(), BaseUrl = "http://localhost:8080" },
    };

    /// <summary>ListSkills returns the attached catalog.</summary>
    [Fact]
    public void ListSkills_WithSkills_ReturnsCatalog()
    {
        var client = new Mock<ISkillRegistryClient>();
        var toolset = new SkillToolset([MakeSkill("PDF Filler")], client.Object);

        var result = toolset.ListSkills();

        result.Should().Contain("PDF Filler");
        result.Should().Contain("does things");
    }

    /// <summary>ListSkills with no skills reports none.</summary>
    [Fact]
    public void ListSkills_WithNoSkills_ReportsNone()
    {
        var client = new Mock<ISkillRegistryClient>();
        var toolset = new SkillToolset([], client.Object);

        toolset.ListSkills().Should().Contain("No skills");
    }

    /// <summary>LoadSkill fetches SKILL.md for a matched skill and caches it.</summary>
    [Fact]
    public async Task LoadSkillAsync_WithMatch_FetchesAndCaches()
    {
        var skill = MakeSkill("PDF Filler", "pdf-filler");
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.GetSkillMarkdownAsync(
                It.IsAny<SkillRegistry>(), "acme", "pdf-filler", "1.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# PDF Filler\nDo X.");

        var toolset = new SkillToolset([skill], client.Object);

        var first = await toolset.LoadSkillAsync("PDF Filler");
        var second = await toolset.LoadSkillAsync("acme/pdf-filler");

        first.Should().Contain("Do X.");
        second.Should().Contain("Do X.");
        client.Verify(c => c.GetSkillMarkdownAsync(
            It.IsAny<SkillRegistry>(), "acme", "pdf-filler", "1.0.0", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>LoadSkill with an unknown name returns a helpful message and does not call the registry.</summary>
    [Fact]
    public async Task LoadSkillAsync_WithUnknownName_ReturnsNotFound()
    {
        var client = new Mock<ISkillRegistryClient>();
        var toolset = new SkillToolset([MakeSkill("PDF Filler")], client.Object);

        var result = await toolset.LoadSkillAsync("nonexistent");

        result.Should().Contain("No attached skill named 'nonexistent'");
        client.Verify(c => c.GetSkillMarkdownAsync(
            It.IsAny<SkillRegistry>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>ReadSkillFile delegates to the registry client for a matched skill.</summary>
    [Fact]
    public async Task ReadSkillFileAsync_WithMatch_ReadsFile()
    {
        var skill = MakeSkill("PDF Filler", "pdf-filler");
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.ReadFileAsync(
                It.IsAny<SkillRegistry>(), "acme", "pdf-filler", "1.0.0", "scripts/run.py", It.IsAny<CancellationToken>()))
            .ReturnsAsync("print('hi')");

        var toolset = new SkillToolset([skill], client.Object);

        var result = await toolset.ReadSkillFileAsync("PDF Filler", "scripts/run.py");

        result.Should().Be("print('hi')");
    }
}
