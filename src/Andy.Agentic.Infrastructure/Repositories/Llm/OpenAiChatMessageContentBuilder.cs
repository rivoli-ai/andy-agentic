using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Infrastructure.Repositories.Llm;

/// <summary>
/// Builds OpenAI-compatible chat message payloads for raw HTTP chat-completions calls.
/// </summary>
internal static class OpenAiChatMessageContentBuilder
{
    public static object? BuildUserContent(string? text, IReadOnlyList<ChatImage>? images)
    {
        var parts = new List<Dictionary<string, object>>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            parts.Add(new Dictionary<string, object>
            {
                ["type"] = "text",
                ["text"] = text,
            });
        }

        if (images is { Count: > 0 })
        {
            foreach (var image in images)
            {
                var dataUri = ToDataUri(image);
                if (dataUri is null)
                {
                    continue;
                }

                parts.Add(new Dictionary<string, object>
                {
                    ["type"] = "image_url",
                    ["image_url"] = new Dictionary<string, object> { ["url"] = dataUri },
                });
            }
        }

        return parts switch
        {
            { Count: 0 } => null,
            { Count: 1 } when parts[0]["type"] is "text" => text!,
            _ => parts,
        };
    }

    public static string BuildAssistantText(ChatHistory message)
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            segments.Add(message.Content.Trim());
        }

        if (message.ToolResults is { Count: > 0 })
        {
            foreach (var result in message.ToolResults)
            {
                segments.Add(result.Success
                    ? $"Tool {result.ToolName}: {result.Result}"
                    : $"Tool {result.ToolName}: Error - {result.ErrorMessage}");
            }
        }

        return string.Join('\n', segments);
    }

    private static string? ToDataUri(ChatImage image)
    {
        if (string.IsNullOrWhiteSpace(image.Data))
        {
            return null;
        }

        if (image.Data.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return image.Data;
        }

        var mimeType = string.IsNullOrWhiteSpace(image.MimeType) ? "image/jpeg" : image.MimeType;
        return $"data:{mimeType};base64,{image.Data}";
    }
}
