namespace Andy.Agentic.Domain.Models;

/// <summary>
///     A single skill hit returned when searching or browsing a skill registry.
/// </summary>
public class SkillSearchResult
{
    /// <summary>
    ///     Gets or sets the registry namespace slug.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the skill (package) slug.
    /// </summary>
    public string SkillSlug { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the resolved version (e.g. latest) for the hit, when known.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the human-readable skill name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the short skill description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
