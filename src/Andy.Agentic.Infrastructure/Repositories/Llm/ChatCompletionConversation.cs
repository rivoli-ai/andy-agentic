using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Mutable OpenAI chat-completions message list used for thinking-model raw HTTP loops.
/// </summary>
internal sealed class ChatCompletionConversation
{
    public List<Dictionary<string, object>> Messages { get; } = new();

    public static ChatCompletionConversation FromHistory(
        IReadOnlyList<ChatHistory> history,
        string? systemInstruction = null)
    {
        var conversation = new ChatCompletionConversation();

        if (!string.IsNullOrWhiteSpace(systemInstruction))
        {
            conversation.Messages.Add(new Dictionary<string, object>
            {
                ["role"] = "system",
                ["content"] = systemInstruction,
            });
        }

        foreach (var message in history.OrderBy(m => m.Timestamp))
        {
            if (message.Role == "user")
            {
                var content = OpenAiChatMessageContentBuilder.BuildUserContent(message.Content, message.Images);
                if (content is null)
                {
                    continue;
                }

                conversation.Messages.Add(new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = content,
                });

                continue;
            }

            if (message.Role != "assistant")
            {
                continue;
            }

            var assistantText = OpenAiChatMessageContentBuilder.BuildAssistantText(message);
            if (string.IsNullOrWhiteSpace(assistantText) && string.IsNullOrWhiteSpace(message.Thinking))
            {
                continue;
            }

            var assistant = new Dictionary<string, object>
            {
                ["role"] = "assistant",
                ["content"] = assistantText,
            };

            if (!string.IsNullOrWhiteSpace(message.Thinking))
            {
                assistant["reasoning_content"] = message.Thinking;
            }

            conversation.Messages.Add(assistant);
        }

        return conversation;
    }

    public void AddAssistantToolTurn(string reasoningContent, string content, IReadOnlyList<ToolCall> toolCalls)
    {
        Messages.Add(new Dictionary<string, object>
        {
            ["role"] = "assistant",
            ["content"] = content ?? string.Empty,
            ["reasoning_content"] = reasoningContent ?? string.Empty,
            ["tool_calls"] = toolCalls.Select(static call => new Dictionary<string, object?>
            {
                ["id"] = call.Id,
                ["type"] = call.Type ?? "function",
                ["function"] = new Dictionary<string, object?>
                {
                    ["name"] = call.Function?.Name ?? string.Empty,
                    ["arguments"] = call.Function?.Arguments ?? "{}",
                },
            }).ToArray(),
        });
    }

    public void AddToolResults(IReadOnlyList<ToolCall> toolCalls, IReadOnlyList<ToolExecutionLog> results)
    {
        for (var i = 0; i < toolCalls.Count; i++)
        {
            var call = toolCalls[i];
            var result = i < results.Count ? results[i] : null;
            var payload = result?.Success == true
                ? result.Result?.ToString() ?? string.Empty
                : result?.ErrorMessage ?? result?.Result?.ToString() ?? "Tool execution failed";

            Messages.Add(new Dictionary<string, object>
            {
                ["role"] = "tool",
                ["tool_call_id"] = call.Id,
                ["content"] = payload,
            });
        }
    }
}
