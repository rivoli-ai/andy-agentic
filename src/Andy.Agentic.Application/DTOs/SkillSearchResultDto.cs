namespace Andy.Agentic.Application.DTOs;

/// <summary>
///     Data Transfer Object for a single skill hit returned from a registry search.
/// </summary>
public class SkillSearchResultDto
{
    /// <summary>Gets or sets the registry namespace slug.</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>Gets or sets the skill (package) slug.</summary>
    public string SkillSlug { get; set; } = string.Empty;

    /// <summary>Gets or sets the resolved version (e.g. latest).</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable skill name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the short skill description.</summary>
    public string Description { get; set; } = string.Empty;
}
