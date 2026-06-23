using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
///     Client for talking to an external agent-skill registry (e.g. andy-skills) over HTTP.
///     All operations take the registry connection so credentials never leave the backend.
/// </summary>
public interface ISkillRegistryClient
{
    /// <summary>
    ///     Searches the registry for skills matching the query.
    /// </summary>
    Task<IReadOnlyList<SkillSearchResult>> SearchAsync(
        SkillRegistry registry,
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetches the full SKILL.md instructions for a specific skill version.
    /// </summary>
    Task<string> GetSkillMarkdownAsync(
        SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the bundled file paths inside a skill version's package.
    /// </summary>
    Task<IReadOnlyList<string>> ListFilesAsync(
        SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the text content of a single bundled file inside a skill version's package.
    /// </summary>
    Task<string> ReadFileAsync(
        SkillRegistry registry,
        string @namespace,
        string skillSlug,
        string version,
        string path,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Verifies that the registry is reachable and credentials are accepted.
    /// </summary>
    Task<bool> TestConnectionAsync(
        SkillRegistry registry,
        CancellationToken cancellationToken = default);
}
