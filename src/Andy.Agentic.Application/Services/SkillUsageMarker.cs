namespace Andy.Agentic.Application.Services;

/// <summary>
///     A reference to a skill attached to an agent, used to resolve a model-declared
///     <c>&lt;skills-used&gt;</c> marker back to display labels.
/// </summary>
public sealed record SkillRef(string Namespace, string Slug, string DisplayName, string Label);

/// <summary>
///     Outcome of inspecting the leading content of a thinking-model reply for the
///     <c>&lt;skills-used&gt;…&lt;/skills-used&gt;</c> marker.
/// </summary>
public enum SkillMarkerState
{
    /// <summary>The buffer may still become a marker — keep buffering before emitting.</summary>
    Incomplete,

    /// <summary>A leading marker was found and parsed.</summary>
    Resolved,

    /// <summary>No leading marker is present — the buffered content is normal output.</summary>
    Absent,
}

/// <summary>
///     Parses and strips the hidden <c>&lt;skills-used&gt;</c> marker that thinking models emit at the
///     start of a reply to declare which attached skills they actually used. Pure and side-effect free
///     so it can be unit tested independently of the streaming pipeline.
/// </summary>
public static class SkillUsageMarker
{
    public const string OpenTag = "<skills-used>";
    public const string CloseTag = "</skills-used>";

    /// <summary>
    ///     Resolves the comma/space-separated tokens inside a marker to the matching skills' labels.
    ///     Tokens may be a slug, a display name, or <c>namespace/slug</c> (case-insensitive). Unknown
    ///     tokens are ignored and duplicates collapsed.
    /// </summary>
    public static List<string> Resolve(string markerInner, IReadOnlyList<SkillRef> skills)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(markerInner))
        {
            return result;
        }

        var tokens = markerInner.Split(
            new[] { ',', ';', '\n', '\r', ' ', '\t' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            var match = skills.FirstOrDefault(s =>
                s.Slug.Equals(token, StringComparison.OrdinalIgnoreCase)
                || s.DisplayName.Equals(token, StringComparison.OrdinalIgnoreCase)
                || $"{s.Namespace}/{s.Slug}".Equals(token, StringComparison.OrdinalIgnoreCase));

            if (match is not null && !result.Contains(match.Label))
            {
                result.Add(match.Label);
            }
        }

        return result;
    }

    /// <summary>
    ///     Inspects accumulated leading reply content for the marker.
    /// </summary>
    /// <param name="buffer">All content received so far (before any has been shown to the user).</param>
    /// <param name="skills">The skills inlined into the prompt.</param>
    /// <param name="labels">Resolved labels when <see cref="SkillMarkerState.Resolved"/>.</param>
    /// <param name="remainder">
    ///     The content to show once the marker is handled: the text after the marker (Resolved) or the
    ///     whole buffer (Absent). Empty while Incomplete.
    /// </param>
    public static SkillMarkerState TryResolveLeading(
        string buffer,
        IReadOnlyList<SkillRef> skills,
        out List<string> labels,
        out string remainder)
    {
        labels = new List<string>();
        remainder = string.Empty;

        var trimmedStart = buffer.TrimStart();
        var couldBeMarker = trimmedStart.Length == 0
            || trimmedStart.StartsWith("<skills-used", StringComparison.OrdinalIgnoreCase)
            || OpenTag.StartsWith(trimmedStart, StringComparison.OrdinalIgnoreCase);

        var closeIdx = buffer.IndexOf(CloseTag, StringComparison.OrdinalIgnoreCase);
        if (closeIdx < 0)
        {
            remainder = buffer;
            return couldBeMarker ? SkillMarkerState.Incomplete : SkillMarkerState.Absent;
        }

        // A close tag exists; only treat it as a leading marker if the open tag is present and
        // preceded by whitespace only.
        var openIdx = buffer.IndexOf(OpenTag, StringComparison.OrdinalIgnoreCase);
        if (openIdx < 0 || openIdx > closeIdx || !string.IsNullOrWhiteSpace(buffer[..openIdx]))
        {
            remainder = buffer;
            return SkillMarkerState.Absent;
        }

        var inner = buffer.Substring(openIdx + OpenTag.Length, closeIdx - openIdx - OpenTag.Length);
        labels = Resolve(inner, skills);
        remainder = buffer[(closeIdx + CloseTag.Length)..].TrimStart('\r', '\n', ' ', '\t');
        return SkillMarkerState.Resolved;
    }

    /// <summary>
    ///     Convenience for a complete (non-streamed) reply: returns the resolved labels and the reply
    ///     with the leading marker stripped. If no marker is present, returns the content unchanged.
    /// </summary>
    public static (List<string> Labels, string Cleaned) Process(string content, IReadOnlyList<SkillRef> skills)
    {
        var state = TryResolveLeading(content, skills, out var labels, out var remainder);
        return state == SkillMarkerState.Resolved ? (labels, remainder) : (new List<string>(), content);
    }
}
