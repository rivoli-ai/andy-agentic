using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Repositories.Llm;
using FluentAssertions;

namespace Andy.Agentic.Infrastructure.Tests.Repositories;

public class ChatCompletionConversationTests
{
    [Fact]
    public void FromHistory_PrependsSystemInstruction()
    {
        var conversation = ChatCompletionConversation.FromHistory([], "You are helpful.");

        conversation.Messages.Should().ContainSingle();
        conversation.Messages[0]["role"].Should().Be("system");
        conversation.Messages[0]["content"].Should().Be("You are helpful.");
    }

    [Fact]
    public void FromHistory_OrdersMessagesByTimestamp()
    {
        var history = new List<ChatHistory>
        {
            new()
            {
                Role = "assistant",
                Content = "second",
                Timestamp = DateTime.UtcNow.AddMinutes(1),
            },
            new()
            {
                Role = "user",
                Content = "first",
                Timestamp = DateTime.UtcNow,
            },
        };

        var conversation = ChatCompletionConversation.FromHistory(history);

        conversation.Messages.Should().HaveCount(2);
        conversation.Messages[0]["role"].Should().Be("user");
        conversation.Messages[1]["role"].Should().Be("assistant");
    }

    [Fact]
    public void FromHistory_UserMessageWithImage_UsesMultimodalContent()
    {
        var history = new List<ChatHistory>
        {
            new()
            {
                Role = "user",
                Content = "look",
                Timestamp = DateTime.UtcNow,
                Images = [new ChatImage { Data = "imgdata", MimeType = "image/png" }],
            },
        };

        var conversation = ChatCompletionConversation.FromHistory(history);

        var content = conversation.Messages[0]["content"];
        content.Should().BeAssignableTo<List<Dictionary<string, object>>>();
    }

    [Fact]
    public void FromHistory_AssistantMessage_IncludesReasoningContent()
    {
        var history = new List<ChatHistory>
        {
            new()
            {
                Role = "assistant",
                Content = "answer",
                Thinking = "internal reasoning",
                Timestamp = DateTime.UtcNow,
            },
        };

        var conversation = ChatCompletionConversation.FromHistory(history);

        conversation.Messages[0]["content"].Should().Be("answer");
        conversation.Messages[0]["reasoning_content"].Should().Be("internal reasoning");
    }

    [Fact]
    public void FromHistory_SkipsEmptyUserMessages()
    {
        var history = new List<ChatHistory>
        {
            new()
            {
                Role = "user",
                Content = "   ",
                Timestamp = DateTime.UtcNow,
            },
        };

        var conversation = ChatCompletionConversation.FromHistory(history);

        conversation.Messages.Should().BeEmpty();
    }
}
