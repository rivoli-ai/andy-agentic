using Andy.Agentic.Domain.Helpers;
using Xunit;

namespace Andy.Agentic.Domain.Tests.Helpers;

/// <summary>Tests for paged reading of skill files (offset/limit windowing).</summary>
public class SkillContentWindowTests
{
    /// <summary>A small file fits in one page and is returned verbatim (no footer).</summary>
    [Fact]
    public void Window_SmallFile_ReturnedWhole()
    {
        Assert.Equal("hello world", SkillContentWindow.Window("hello world", 0, 0, "a.txt"));
    }

    /// <summary>A large file returns the first page with a continuation footer pointing to the next offset.</summary>
    [Fact]
    public void Window_LargeFile_FirstPage_HasContinuation()
    {
        var content = new string('x', 100_000);

        var page = SkillContentWindow.Window(content, 0, 40_000, "big.txt");

        Assert.StartsWith(new string('x', 40_000), page);
        Assert.Contains("Showing characters 0-40000 of 100000", page);
        Assert.Contains("offset=40000", page);
    }

    /// <summary>Reading from an offset returns the next slice and marks the end when reached.</summary>
    [Fact]
    public void Window_SecondPage_ReachesEnd()
    {
        var content = new string('x', 50_000);

        var page = SkillContentWindow.Window(content, 40_000, 40_000, "big.txt");

        Assert.Contains("Showing characters 40000-50000 of 50000", page);
        Assert.Contains("End of file.", page);
        Assert.DoesNotContain("offset=50000", page);
    }

    /// <summary>The page size is capped at MaxLimit even if a larger limit is requested.</summary>
    [Fact]
    public void Window_LimitCappedAtMax()
    {
        var content = new string('x', 300_000);

        var page = SkillContentWindow.Window(content, 0, 1_000_000, "big.txt");

        Assert.Contains($"Showing characters 0-{SkillContentWindow.MaxLimit} of 300000", page);
    }

    /// <summary>A negative offset is clamped to 0; a non-positive limit uses the default.</summary>
    [Fact]
    public void Window_NegativeOffset_And_DefaultLimit()
    {
        var content = new string('a', SkillContentWindow.DefaultLimit + 10);

        var page = SkillContentWindow.Window(content, -5, 0, "f.txt");

        Assert.StartsWith(new string('a', SkillContentWindow.DefaultLimit), page);
        Assert.Contains($"Showing characters 0-{SkillContentWindow.DefaultLimit}", page);
    }

    /// <summary>An offset past the end is reported, not crashed.</summary>
    [Fact]
    public void Window_OffsetPastEnd_Reports()
    {
        Assert.Contains("past the end", SkillContentWindow.Window("short", 100, 40_000, "f.txt"));
    }

    /// <summary>Empty content is handled.</summary>
    [Fact]
    public void Window_Empty_Handled()
    {
        Assert.Contains("empty", SkillContentWindow.Window("", 0, 0, "f.txt"));
    }
}
