using Andy.Agentic.Infrastructure.Services.SkillRegistry;
using FluentAssertions;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Semantic;

/// <summary>
///     Tests for tolerant resolution of a skill file path against the package tree — covering the
///     case where SKILL.md references a file by a path that is not the full ZIP entry path.
/// </summary>
public class SkillRegistryClientResolvePathTests
{
    private static readonly string[] FlatPackage = ["SKILL.md", "reference.md", "scripts/run.py"];

    private static readonly string[] NestedPackage =
        ["my-skill/SKILL.md", "my-skill/reference.md", "my-skill/templates/base.mjml"];

    /// <summary>Exact path resolves to itself.</summary>
    [Fact]
    public void Resolve_ExactPath_ReturnsIt()
    {
        SkillRegistryClient.ResolveEntryPath(FlatPackage, "scripts/run.py").Should().Be("scripts/run.py");
    }

    /// <summary>Matching is case-insensitive.</summary>
    [Fact]
    public void Resolve_CaseInsensitive()
    {
        SkillRegistryClient.ResolveEntryPath(FlatPackage, "Reference.MD").Should().Be("reference.md");
    }

    /// <summary>A leading ./ or / is tolerated.</summary>
    [Theory]
    [InlineData("./reference.md")]
    [InlineData("/reference.md")]
    public void Resolve_LeadingDotOrSlash(string requested)
    {
        SkillRegistryClient.ResolveEntryPath(FlatPackage, requested).Should().Be("reference.md");
    }

    /// <summary>
    ///     The package sits under a top-level folder; a file referenced relative to SKILL.md resolves
    ///     against the SKILL.md root.
    /// </summary>
    [Fact]
    public void Resolve_NestedPackage_RelativeToSkillRoot()
    {
        SkillRegistryClient.ResolveEntryPath(NestedPackage, "reference.md").Should().Be("my-skill/reference.md");
    }

    /// <summary>A nested subpath relative to SKILL.md resolves too.</summary>
    [Fact]
    public void Resolve_NestedPackage_RelativeSubPath()
    {
        SkillRegistryClient.ResolveEntryPath(NestedPackage, "templates/base.mjml")
            .Should().Be("my-skill/templates/base.mjml");
    }

    /// <summary>A bare filename resolves by unique basename.</summary>
    [Fact]
    public void Resolve_ByUniqueFilename()
    {
        SkillRegistryClient.ResolveEntryPath(NestedPackage, "base.mjml").Should().Be("my-skill/templates/base.mjml");
    }

    /// <summary>An ambiguous filename (present twice) is not guessed.</summary>
    [Fact]
    public void Resolve_AmbiguousFilename_ReturnsNull()
    {
        string[] paths = ["a/config.json", "b/config.json"];
        SkillRegistryClient.ResolveEntryPath(paths, "config.json").Should().BeNull();
    }

    /// <summary>A truly missing file resolves to null.</summary>
    [Fact]
    public void Resolve_Missing_ReturnsNull()
    {
        SkillRegistryClient.ResolveEntryPath(FlatPackage, "does/not/exist.txt").Should().BeNull();
    }

    /// <summary>Empty inputs are handled.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_EmptyRequested_ReturnsNull(string requested)
    {
        SkillRegistryClient.ResolveEntryPath(FlatPackage, requested).Should().BeNull();
    }
}
