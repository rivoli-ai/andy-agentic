using Andy.Agentic.Application.Services;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Andy.Agentic.Application.Tests.Services;

/// <summary>
///     Tests for <see cref="SkillToolCalling"/> — on-demand skill loading (progressive disclosure)
///     on the thinking-model path.
/// </summary>
public class SkillToolCallingTests
{
    private static List<AgentSkill> Skills =>
    [
        new()
        {
            Namespace = "mynamesapce", SkillSlug = "pptx", Version = "1.0.0",
            DisplayName = "powerpoint", Description = "PowerPoint decks", IsActive = true,
            Registry = new SkillRegistry { BaseUrl = "http://localhost:8080" },
        },
        new()
        {
            Namespace = "mynamesapce", SkillSlug = "email", Version = "1.0.0",
            DisplayName = "Email", Description = "MJML emails", IsActive = true,
            Registry = new SkillRegistry { BaseUrl = "http://localhost:8080" },
        },
    ];

    private static ToolCall Call(string name, string argsJson) => new()
    {
        Id = "call_1",
        Function = new ToolCallFunction { Name = name, Arguments = argsJson },
    };

    /// <summary>Only the two skill functions are recognized.</summary>
    [Theory]
    [InlineData("load_skill", true)]
    [InlineData("read_skill_file", true)]
    [InlineData("get-library-docs", false)]
    [InlineData("", false)]
    public void IsSkillTool_RecognizesSkillFunctions(string name, bool expected)
    {
        SkillToolCalling.IsSkillTool(name).Should().Be(expected);
    }

    /// <summary>Both skill tool schemas are advertised with the right required args.</summary>
    [Fact]
    public void BuildToolSchemas_ExposesLoadAndReadFunctions()
    {
        var schemas = SkillToolCalling.BuildToolSchemas();

        schemas.Select(s => s.Function.Name).Should().BeEquivalentTo("load_skill", "read_skill_file");
        schemas.Single(s => s.Function.Name == "load_skill").Function.Parameters.Required.Should().Contain("skill");
        schemas.Single(s => s.Function.Name == "read_skill_file").Function.Parameters.Required.Should().Contain(new[] { "skill", "path" });
    }

    /// <summary>The catalog contains only name + description (cheap level-1 metadata).</summary>
    [Fact]
    public void BuildCatalogPrompt_ListsNameAndDescriptionOnly()
    {
        var catalog = SkillToolCalling.BuildCatalogPrompt(Skills);

        catalog.Should().Contain("powerpoint: PowerPoint decks");
        catalog.Should().Contain("Email: MJML emails");
        catalog.Should().Contain("load_skill");
    }

    /// <summary>No active skills → no catalog.</summary>
    [Fact]
    public void BuildCatalogPrompt_NoSkills_ReturnsNull()
    {
        SkillToolCalling.BuildCatalogPrompt(new List<AgentSkill>()).Should().BeNull();
    }

    /// <summary>load_skill fetches the SKILL.md on demand and reports the loaded label.</summary>
    [Fact]
    public async Task ExecuteAsync_LoadSkill_FetchesMarkdownAndReportsLabel()
    {
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.GetSkillMarkdownAsync(
                It.IsAny<SkillRegistry>(), "mynamesapce", "pptx", "1.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# PowerPoint skill\nDo X.");

        var (log, label) = await SkillToolCalling.ExecuteAsync(
            Call("load_skill", "{\"skill\":\"powerpoint\"}"), Skills, client.Object, Guid.NewGuid(), "s1", default);

        log.Success.Should().BeTrue();
        log.Result.Should().Be("# PowerPoint skill\nDo X.");
        log.ToolId.Should().Be(Guid.Empty); // synthetic, not persisted
        label.Should().Be("powerpoint (mynamesapce/pptx@1.0.0)");
    }

    /// <summary>load_skill matches by slug too.</summary>
    [Fact]
    public async Task ExecuteAsync_LoadSkill_MatchesBySlug()
    {
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.GetSkillMarkdownAsync(It.IsAny<SkillRegistry>(), "mynamesapce", "email", "1.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Email");

        var (log, label) = await SkillToolCalling.ExecuteAsync(
            Call("load_skill", "{\"skill\":\"email\"}"), Skills, client.Object, null, null, default);

        log.Success.Should().BeTrue();
        label.Should().Contain("Email");
    }

    /// <summary>An unknown skill name returns a helpful failure and never calls the registry.</summary>
    [Fact]
    public async Task ExecuteAsync_UnknownSkill_FailsWithoutRegistryCall()
    {
        var client = new Mock<ISkillRegistryClient>();

        var (log, label) = await SkillToolCalling.ExecuteAsync(
            Call("load_skill", "{\"skill\":\"nope\"}"), Skills, client.Object, null, null, default);

        log.Success.Should().BeFalse();
        log.Result!.ToString().Should().Contain("No attached skill named 'nope'");
        label.Should().BeNull();
        client.Verify(c => c.GetSkillMarkdownAsync(It.IsAny<SkillRegistry>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>read_skill_file reads a bundled file on demand.</summary>
    [Fact]
    public async Task ExecuteAsync_ReadSkillFile_ReadsFile()
    {
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.ReadFileAsync(
                It.IsAny<SkillRegistry>(), "mynamesapce", "pptx", "1.0.0", "templates/base.pptx.md", It.IsAny<CancellationToken>()))
            .ReturnsAsync("template body");

        var (log, label) = await SkillToolCalling.ExecuteAsync(
            Call("read_skill_file", "{\"skill\":\"powerpoint\",\"path\":\"templates/base.pptx.md\"}"),
            Skills, client.Object, null, null, default);

        log.Success.Should().BeTrue();
        log.Result.Should().Be("template body");
        label.Should().BeNull(); // reading a file is not "loading a skill"
    }

    /// <summary>read_skill_file without a path fails clearly.</summary>
    [Fact]
    public async Task ExecuteAsync_ReadSkillFile_MissingPath_Fails()
    {
        var client = new Mock<ISkillRegistryClient>();

        var (log, _) = await SkillToolCalling.ExecuteAsync(
            Call("read_skill_file", "{\"skill\":\"powerpoint\"}"), Skills, client.Object, null, null, default);

        log.Success.Should().BeFalse();
        log.Result!.ToString().Should().Contain("Missing 'path'");
    }

    /// <summary>Registry errors are surfaced as a failed log, not thrown.</summary>
    [Fact]
    public async Task ExecuteAsync_RegistryThrows_ReturnsFailureLog()
    {
        var client = new Mock<ISkillRegistryClient>();
        client.Setup(c => c.GetSkillMarkdownAsync(It.IsAny<SkillRegistry>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var (log, _) = await SkillToolCalling.ExecuteAsync(
            Call("load_skill", "{\"skill\":\"powerpoint\"}"), Skills, client.Object, null, null, default);

        log.Success.Should().BeFalse();
        log.Result!.ToString().Should().Contain("Failed to execute load_skill");
    }
}
