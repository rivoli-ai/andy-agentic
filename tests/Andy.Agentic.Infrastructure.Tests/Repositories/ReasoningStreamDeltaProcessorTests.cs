using Andy.Agentic.Infrastructure.Repositories.Llm;
using FluentAssertions;

namespace Andy.Agentic.Infrastructure.Tests.Repositories;

public class ReasoningStreamDeltaProcessorTests
{
    [Fact]
    public void Process_VllmReasoningField_EmitsThinkingChunk()
    {
        var processor = new ReasoningStreamDeltaProcessor();

        var chunks = processor.Process(null, "Let me think", string.Empty).ToList();

        chunks.Should().ContainSingle();
        chunks[0].Thinking.Should().Be("Let me think");
        chunks[0].Content.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Process_DashScopeReasoningContentField_EmitsThinkingChunk()
    {
        var processor = new ReasoningStreamDeltaProcessor();

        var chunks = processor.Process("Step 1", null, string.Empty).ToList();

        chunks.Should().ContainSingle();
        chunks[0].Thinking.Should().Be("Step 1");
    }

    [Fact]
    public void Process_InlineThinkTags_SplitsThinkingAndAnswer()
    {
        var processor = new ReasoningStreamDeltaProcessor();

        var chunks = processor
            .Process(null, null, "\u003cthink\u003eHidden\u003c/think\u003eVisible")
            .ToList();

        chunks.Should().HaveCount(2);
        chunks[0].Thinking.Should().Be("Hidden");
        chunks[1].Content.Should().Be("Visible");
    }
}
