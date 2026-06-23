using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Repositories.Llm;
using FluentAssertions;

namespace Andy.Agentic.Infrastructure.Tests.Repositories;

public class OpenAiChatMessageContentBuilderTests
{
    [Fact]
    public void BuildUserContent_TextOnly_ReturnsPlainString()
    {
        var content = OpenAiChatMessageContentBuilder.BuildUserContent("hello", null);

        content.Should().Be("hello");
    }

    [Fact]
    public void BuildUserContent_TextAndImage_ReturnsMultimodalParts()
    {
        var content = OpenAiChatMessageContentBuilder.BuildUserContent(
            "describe this",
            [new ChatImage { Data = "abc123", MimeType = "image/png" }]);

        var parts = content.Should().BeAssignableTo<List<Dictionary<string, object>>>().Subject;
        parts.Should().HaveCount(2);
        parts[0]["type"].Should().Be("text");
        parts[1]["type"].Should().Be("image_url");
    }

    [Fact]
    public void BuildUserContent_ImageOnly_ReturnsImagePart()
    {
        var content = OpenAiChatMessageContentBuilder.BuildUserContent(
            null,
            [new ChatImage { Data = "abc123", MimeType = "image/jpeg" }]);

        var parts = content.Should().BeAssignableTo<List<Dictionary<string, object>>>().Subject;
        parts.Should().ContainSingle();
        parts[0]["type"].Should().Be("image_url");
    }

    [Fact]
    public void BuildAssistantText_IncludesContentAndToolResults()
    {
        var message = new ChatHistory
        {
            Content = "Done.",
            ToolResults =
            [
                new ToolExecutionLog
                {
                    ToolName = "search",
                    Success = true,
                    Result = "found 3 items",
                    Tool = new Tool(),
                },
                new ToolExecutionLog
                {
                    ToolName = "export",
                    Success = false,
                    ErrorMessage = "timeout",
                    Tool = new Tool(),
                },
            ],
        };

        var text = OpenAiChatMessageContentBuilder.BuildAssistantText(message);

        text.Should().Contain("Done.");
        text.Should().Contain("Tool search: found 3 items");
        text.Should().Contain("Tool export: Error - timeout");
    }
}
