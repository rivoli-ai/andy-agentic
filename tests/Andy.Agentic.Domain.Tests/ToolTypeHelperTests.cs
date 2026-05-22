using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Models.Semantic;

namespace Andy.Agentic.Domain.Tests;

public class ToolTypeHelperTests
{
    [Theory]
    [InlineData("McpTool", true)]
    [InlineData("mcptool", true)]
    [InlineData("mcp", true)]
    [InlineData("ApiTool", false)]
    public void Matches_McpTool_AcceptsKnownAliases(string toolType, bool expected) =>
        Assert.Equal(expected, ToolTypeHelper.Matches(ToolType.McpTool, toolType));

    [Theory]
    [InlineData("ApiTool", true)]
    [InlineData("api", true)]
    [InlineData("McpTool", false)]
    public void Matches_ApiTool_AcceptsKnownAliases(string toolType, bool expected) =>
        Assert.Equal(expected, ToolTypeHelper.Matches(ToolType.ApiTool, toolType));
}
