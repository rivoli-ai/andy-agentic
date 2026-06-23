using Andy.Agentic.Application.Services;
using FluentAssertions;
using Xunit;

namespace Andy.Agentic.Application.Tests.Services;

/// <summary>
///     Exhaustive tests for <see cref="SkillUsageMarker"/> — how the model-declared
///     <c>&lt;skills-used&gt;</c> marker is resolved, stripped, and streamed across many scenarios.
/// </summary>
public class SkillUsageMarkerTests
{
    private static readonly List<SkillRef> Skills =
    [
        new("mynamesapce", "pptx", "powerpoint", "powerpoint (mynamesapce/pptx@1.0.0)"),
        new("mynamesapce", "email", "Email", "Email (mynamesapce/email@1.0.0)"),
        new("acme", "pdf-filler", "PDF Filler", "PDF Filler (acme/pdf-filler@2.1.0)"),
    ];

    // ---------------------------------------------------------------------
    // Resolve: token -> label matching
    // ---------------------------------------------------------------------

    /// <summary>A single slug resolves to its label.</summary>
    [Fact]
    public void Resolve_SingleSlug_ReturnsLabel()
    {
        SkillUsageMarker.Resolve("pptx", Skills)
            .Should().ContainSingle().Which.Should().Be("powerpoint (mynamesapce/pptx@1.0.0)");
    }

    /// <summary>Multiple comma-separated slugs resolve in order.</summary>
    [Fact]
    public void Resolve_MultipleCommaSeparated_ReturnsAll()
    {
        SkillUsageMarker.Resolve("pptx, email", Skills)
            .Should().Equal("powerpoint (mynamesapce/pptx@1.0.0)", "Email (mynamesapce/email@1.0.0)");
    }

    /// <summary>Whitespace, semicolons and newlines all separate tokens.</summary>
    [Theory]
    [InlineData("pptx email")]
    [InlineData("pptx;email")]
    [InlineData("pptx\nemail")]
    [InlineData("  pptx ,  email  ")]
    public void Resolve_VariousSeparators_ReturnsBoth(string inner)
    {
        SkillUsageMarker.Resolve(inner, Skills).Should().HaveCount(2);
    }

    /// <summary>Matching is case-insensitive.</summary>
    [Theory]
    [InlineData("PPTX")]
    [InlineData("Pptx")]
    [InlineData("POWERPOINT")]
    public void Resolve_CaseInsensitive_Matches(string token)
    {
        SkillUsageMarker.Resolve(token, Skills).Should().ContainSingle();
    }

    /// <summary>A single-word display name resolves to the label.</summary>
    [Fact]
    public void Resolve_BySingleWordDisplayName_Matches()
    {
        SkillUsageMarker.Resolve("Email", Skills)
            .Should().ContainSingle().Which.Should().Be("Email (mynamesapce/email@1.0.0)");
    }

    /// <summary>
    ///     A multi-word display name does NOT match by name (tokens are space-separated), but its slug
    ///     still does — which is what the model is instructed to emit.
    /// </summary>
    [Fact]
    public void Resolve_MultiWordDisplayName_OnlyMatchesBySlug()
    {
        SkillUsageMarker.Resolve("PDF Filler", Skills).Should().BeEmpty();
        SkillUsageMarker.Resolve("pdf-filler", Skills)
            .Should().ContainSingle().Which.Should().Be("PDF Filler (acme/pdf-filler@2.1.0)");
    }

    /// <summary>A namespace/slug coordinate resolves to the label.</summary>
    [Fact]
    public void Resolve_ByNamespaceSlug_Matches()
    {
        SkillUsageMarker.Resolve("acme/pdf-filler", Skills)
            .Should().ContainSingle().Which.Should().Be("PDF Filler (acme/pdf-filler@2.1.0)");
    }

    /// <summary>Unknown tokens are ignored, known ones still resolve.</summary>
    [Fact]
    public void Resolve_UnknownTokensIgnored()
    {
        SkillUsageMarker.Resolve("pptx, bogus, nonsense", Skills)
            .Should().ContainSingle().Which.Should().Be("powerpoint (mynamesapce/pptx@1.0.0)");
    }

    /// <summary>Duplicate tokens collapse to a single label.</summary>
    [Fact]
    public void Resolve_Duplicates_Deduped()
    {
        SkillUsageMarker.Resolve("pptx, pptx, powerpoint", Skills).Should().ContainSingle();
    }

    /// <summary>Empty or whitespace inner content resolves to nothing.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Resolve_Empty_ReturnsNothing(string inner)
    {
        SkillUsageMarker.Resolve(inner, Skills).Should().BeEmpty();
    }

    // ---------------------------------------------------------------------
    // TryResolveLeading: streaming buffer states
    // ---------------------------------------------------------------------

    /// <summary>A complete leading marker resolves and the remainder is the rest of the reply.</summary>
    [Fact]
    public void TryResolveLeading_CompleteMarker_ResolvesAndStrips()
    {
        var buffer = "<skills-used>pptx</skills-used>\nHere is your deck.";

        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out var labels, out var remainder);

        state.Should().Be(SkillMarkerState.Resolved);
        labels.Should().ContainSingle().Which.Should().Be("powerpoint (mynamesapce/pptx@1.0.0)");
        remainder.Should().Be("Here is your deck.");
    }

    /// <summary>Leading whitespace/newlines before the marker are tolerated and stripped.</summary>
    [Fact]
    public void TryResolveLeading_LeadingWhitespace_Resolves()
    {
        var buffer = "\n  <skills-used>email</skills-used>\n\nHello";

        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out var labels, out var remainder);

        state.Should().Be(SkillMarkerState.Resolved);
        labels.Should().ContainSingle().Which.Should().Be("Email (mynamesapce/email@1.0.0)");
        remainder.Should().Be("Hello");
    }

    /// <summary>An empty marker resolves with no labels (model used no skill).</summary>
    [Fact]
    public void TryResolveLeading_EmptyMarker_ResolvedNoLabels()
    {
        var buffer = "<skills-used></skills-used>Plain answer.";

        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out var labels, out var remainder);

        state.Should().Be(SkillMarkerState.Resolved);
        labels.Should().BeEmpty();
        remainder.Should().Be("Plain answer.");
    }

    /// <summary>A partially-received marker keeps buffering.</summary>
    [Theory]
    [InlineData("<")]
    [InlineData("<skills")]
    [InlineData("<skills-used>")]
    [InlineData("<skills-used>ppt")]
    [InlineData("<skills-used>pptx</skills-")]
    [InlineData("")]
    [InlineData("   ")]
    public void TryResolveLeading_PartialMarker_Incomplete(string buffer)
    {
        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out _, out _);
        state.Should().Be(SkillMarkerState.Incomplete);
    }

    /// <summary>Content that does not start with the marker is treated as normal output.</summary>
    [Theory]
    [InlineData("Hello, here is your answer.")]
    [InlineData("Sure! ")]
    [InlineData("1. First step")]
    public void TryResolveLeading_NoMarker_Absent(string buffer)
    {
        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out _, out var remainder);
        state.Should().Be(SkillMarkerState.Absent);
        remainder.Should().Be(buffer);
    }

    /// <summary>A marker not at the start (prose first) is not treated as a leading marker.</summary>
    [Fact]
    public void TryResolveLeading_MarkerNotAtStart_Absent()
    {
        var buffer = "Here you go <skills-used>pptx</skills-used>";

        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out _, out var remainder);

        state.Should().Be(SkillMarkerState.Absent);
        remainder.Should().Be(buffer);
    }

    /// <summary>Multiple skills declared in one marker all resolve.</summary>
    [Fact]
    public void TryResolveLeading_MultipleSkills_AllResolved()
    {
        var buffer = "<skills-used>pptx, email</skills-used>Done.";

        SkillUsageMarker.TryResolveLeading(buffer, Skills, out var labels, out var remainder);

        labels.Should().HaveCount(2);
        remainder.Should().Be("Done.");
    }

    /// <summary>Case-variant tags are still recognized.</summary>
    [Fact]
    public void TryResolveLeading_UppercaseTags_Resolves()
    {
        var buffer = "<SKILLS-USED>pptx</SKILLS-USED>Body";

        var state = SkillUsageMarker.TryResolveLeading(buffer, Skills, out var labels, out var remainder);

        state.Should().Be(SkillMarkerState.Resolved);
        labels.Should().ContainSingle();
        remainder.Should().Be("Body");
    }

    /// <summary>When no skills are attached, declared tokens resolve to nothing.</summary>
    [Fact]
    public void TryResolveLeading_NoAttachedSkills_NoLabels()
    {
        var buffer = "<skills-used>pptx</skills-used>Body";

        SkillUsageMarker.TryResolveLeading(buffer, new List<SkillRef>(), out var labels, out var remainder);

        labels.Should().BeEmpty();
        remainder.Should().Be("Body");
    }

    // ---------------------------------------------------------------------
    // Streaming simulation: feed content token-by-token
    // ---------------------------------------------------------------------

    /// <summary>
    ///     Simulates the streaming pipeline: feeds content in small pieces and asserts the marker is
    ///     stripped from the visible output while the skills are reported exactly once.
    /// </summary>
    [Theory]
    [InlineData(new[] { "<skills-used>pptx</skills-used>", "Here ", "is ", "your deck." }, "Here is your deck.")]
    [InlineData(new[] { "<skills", "-used>pp", "tx</ski", "lls-used>", "Body text" }, "Body text")]
    [InlineData(new[] { "<skills-used>pptx</skills-used>Body text" }, "Body text")]
    [InlineData(new[] { "No marker ", "at all here." }, "No marker at all here.")]
    public void StreamingSimulation_StripsMarker_AndReassemblesBody(string[] chunks, string expectedVisible)
    {
        var visible = new System.Text.StringBuilder();
        var reportedSkills = new List<string>();
        var markerDone = false;
        var buffer = new System.Text.StringBuilder();

        foreach (var chunk in chunks)
        {
            if (markerDone)
            {
                visible.Append(chunk);
                continue;
            }

            buffer.Append(chunk);
            var state = SkillUsageMarker.TryResolveLeading(buffer.ToString(), Skills, out var labels, out var remainder);

            if (state == SkillMarkerState.Resolved)
            {
                reportedSkills.AddRange(labels);
                markerDone = true;
                buffer.Clear();
                visible.Append(remainder);
            }
            else if (state == SkillMarkerState.Absent)
            {
                markerDone = true;
                buffer.Clear();
                visible.Append(remainder);
            }
            // Incomplete: keep buffering
        }

        if (!markerDone)
        {
            visible.Append(buffer);
        }

        visible.ToString().Should().Be(expectedVisible);
        visible.ToString().Should().NotContain("skills-used");
    }

    /// <summary>The streaming simulation reports the declared skill exactly once.</summary>
    [Fact]
    public void StreamingSimulation_ReportsSkillOnce()
    {
        var chunks = new[] { "<skills-used>", "pptx", "</skills-used>", "Deck ready" };
        var reported = new List<string>();
        var markerDone = false;
        var buffer = new System.Text.StringBuilder();

        foreach (var chunk in chunks)
        {
            if (markerDone)
            {
                continue;
            }

            buffer.Append(chunk);
            var state = SkillUsageMarker.TryResolveLeading(buffer.ToString(), Skills, out var labels, out _);
            if (state == SkillMarkerState.Resolved)
            {
                reported.AddRange(labels);
                markerDone = true;
            }
            else if (state == SkillMarkerState.Absent)
            {
                markerDone = true;
            }
        }

        reported.Should().ContainSingle().Which.Should().Be("powerpoint (mynamesapce/pptx@1.0.0)");
    }

    // ---------------------------------------------------------------------
    // Process: whole-reply convenience
    // ---------------------------------------------------------------------

    /// <summary>Process returns labels and the cleaned reply for a complete message.</summary>
    [Fact]
    public void Process_CompleteReply_ReturnsLabelsAndCleaned()
    {
        var (labels, cleaned) = SkillUsageMarker.Process("<skills-used>email</skills-used>\nDraft below.", Skills);

        labels.Should().ContainSingle().Which.Should().Be("Email (mynamesapce/email@1.0.0)");
        cleaned.Should().Be("Draft below.");
    }

    /// <summary>Process leaves content unchanged when there is no marker.</summary>
    [Fact]
    public void Process_NoMarker_ReturnsContentUnchanged()
    {
        var (labels, cleaned) = SkillUsageMarker.Process("Just a normal answer.", Skills);

        labels.Should().BeEmpty();
        cleaned.Should().Be("Just a normal answer.");
    }
}
