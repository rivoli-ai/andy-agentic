namespace Andy.Agentic.Domain.Helpers;

/// <summary>
///     Returns a windowed slice of a skill file so the model can page through large bundled files
///     (offset + limit), the way agent file-read tools do — without ever discarding data. The
///     returned text carries a footer telling the model how much remains and how to continue.
/// </summary>
public static class SkillContentWindow
{
    /// <summary>Default number of characters returned per page (~10k tokens).</summary>
    public const int DefaultLimit = 40_000;

    /// <summary>Hard cap on a single page to protect the context window (~20k tokens).</summary>
    public const int MaxLimit = 80_000;

    /// <summary>
    ///     Returns characters <paramref name="offset"/>..<paramref name="offset"/>+<paramref name="limit"/>
    ///     of <paramref name="content"/>, with a continuation footer when the file is longer.
    /// </summary>
    /// <param name="content">The full file content.</param>
    /// <param name="offset">Zero-based start character (clamped to ≥ 0).</param>
    /// <param name="limit">Max characters to return (≤ 0 ⇒ default; capped at <see cref="MaxLimit"/>).</param>
    /// <param name="label">A label (the file path) used in the footer.</param>
    public static string Window(string? content, int offset, int limit, string label)
    {
        content ??= string.Empty;

        if (offset < 0)
        {
            offset = 0;
        }

        if (limit <= 0)
        {
            limit = DefaultLimit;
        }

        limit = Math.Min(limit, MaxLimit);

        var total = content.Length;

        if (offset >= total)
        {
            return total == 0
                ? $"['{label}' is empty.]"
                : $"[Offset {offset} is past the end of '{label}' ({total} characters). The file ends at {total}.]";
        }

        var end = Math.Min(offset + limit, total);
        var slice = content[offset..end];

        // Whole file fits in one page → return it as-is, no footer noise.
        if (offset == 0 && end == total)
        {
            return slice;
        }

        var footer = end < total
            ? $"\n\n[Showing characters {offset}-{end} of {total} for '{label}'. " +
              $"Call read_skill_file again with offset={end} to read the next part.]"
            : $"\n\n[Showing characters {offset}-{end} of {total} for '{label}'. End of file.]";

        return slice + footer;
    }
}
