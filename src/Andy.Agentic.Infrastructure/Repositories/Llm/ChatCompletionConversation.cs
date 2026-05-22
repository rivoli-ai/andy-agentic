using System.Text.Json;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Mutable OpenAI chat-completions message list used for thinking-model raw HTTP loops.
/// </summary>
internal sealed class ChatCompletionConversation
{
    public List<Dictionary<string, object>> Messages { get; } = new();

    public static ChatCompletionConversation FromHistory(IReadOnlyList<ChatHistory> history)
    {
        var conversation = new ChatCompletionConversation();

        foreach (var message in history.OrderBy(m => m.Timestamp))
        {
            if (message.Role == "user")
            {
                if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    conversation.Messages.Add(new Dictionary<string, object>
                    {
                        ["role"] = "user",
                        ["content"] = message.Content,
                    });
                }

                continue;
            }

            if (message.Role == "assistant" && !string.IsNullOrWhiteSpace(message.Content))
            {
                var assistant = new Dictionary<string, object>
                {
                    ["role"] = "assistant",
                    ["content"] = message.Content,
                };

                if (!string.IsNullOrWhiteSpace(message.Thinking))
                {
                    assistant["reasoning_content"] = message.Thinking;
                }

                conversation.Messages.Add(assistant);
            }
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
