using System.Text.Json;
using System.Text.Json.Nodes;
using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Patches OpenAI-compatible chat completion JSON for Kimi/Qwen hybrid thinking + tool-call quirks.
/// </summary>
internal static class ThinkingModelRequestPatcher
{
    private const string ChatCompletionsSegment = "chat/completions";

    private static readonly JsonSerializerOptions OutputOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static bool ShouldPatch(Uri? requestUri) =>
        requestUri?.AbsolutePath.Contains(ChatCompletionsSegment, StringComparison.OrdinalIgnoreCase) == true;

    public static string PatchChatCompletionsRequest(string requestJson)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(requestJson);
        }
        catch (JsonException)
        {
            return requestJson;
        }

        if (root is not JsonObject body)
        {
            return requestJson;
        }

        var model = body["model"]?.GetValue<string>();
        var hasTools = body["tools"] is JsonArray { Count: > 0 };
        var isThinkingModel = ThinkingModelSupport.SupportsReasoningContentStream(model);

        if (!isThinkingModel && !hasTools)
        {
            return requestJson;
        }

        var modified = false;
        var hasToolCallsInHistory = false;

        if (isThinkingModel && body["messages"] is JsonArray messages)
        {
            foreach (var node in messages)
            {
                if (node is not JsonObject message)
                {
                    continue;
                }

                if (!string.Equals(message["role"]?.GetValue<string>(), "assistant", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (message["tool_calls"] is not JsonArray { Count: > 0 })
                {
                    continue;
                }

                hasToolCallsInHistory = true;
                if (message["reasoning_content"] is null)
                {
                    message["reasoning_content"] = string.Empty;
                    modified = true;
                }
            }
        }

        if (isThinkingModel && hasToolCallsInHistory)
        {
            var overrides = ThinkingModelSupport.GetToolPathThinkingDisableOverrides(new LlmConfig { Model = model! });
            if (overrides != null)
            {
                foreach (var (key, value) in overrides)
                {
                    body[key] = ToJsonNode(value);
                    modified = true;
                }
            }
        }

        // Moonshot rejects scalar enum values — always normalize tool schemas when tools are present.
        if (hasTools && MoonshotToolSchemaSanitizer.SanitizeToolsNode(body["tools"]))
        {
            modified = true;
        }

        return modified || hasTools
            ? root.ToJsonString(OutputOptions)
            : requestJson;
    }

    private static JsonNode? ToJsonNode(object value) =>
        value switch
        {
            null => null,
            JsonNode node => node,
            _ => JsonSerializer.SerializeToNode(value),
        };
}
