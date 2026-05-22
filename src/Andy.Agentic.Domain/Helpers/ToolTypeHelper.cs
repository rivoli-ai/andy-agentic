using Andy.Agentic.Domain.Models.Semantic;

namespace Andy.Agentic.Domain.Helpers;

/// <summary>
/// Normalizes persisted tool type strings for provider/factory lookup.
/// </summary>
public static class ToolTypeHelper
{
    public static bool Matches(ToolType expected, string? toolType)
    {
        if (string.IsNullOrWhiteSpace(toolType))
        {
            return false;
        }

        var normalized = toolType.Trim();

        if (Enum.TryParse<ToolType>(normalized, ignoreCase: true, out var parsed))
        {
            return parsed == expected;
        }

        return expected switch
        {
            ToolType.McpTool => string.Equals(normalized, "mcp", StringComparison.OrdinalIgnoreCase),
            ToolType.ApiTool => string.Equals(normalized, "api", StringComparison.OrdinalIgnoreCase),
            ToolType.InternalTool => string.Equals(normalized, "internal", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    public static bool TryParse(string? toolType, out ToolType parsed)
    {
        parsed = default;
        return !string.IsNullOrWhiteSpace(toolType)
               && Enum.TryParse(toolType.Trim(), ignoreCase: true, out parsed);
    }
}
