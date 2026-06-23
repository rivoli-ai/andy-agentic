using System.Text;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal;

/// <summary>
///     Backs the per-agent <c>skills</c> plugin functions (list_skills / load_skill /
///     read_skill_file). Implements Anthropic-style progressive disclosure: the model sees a
///     cheap catalog, then pulls full SKILL.md instructions and bundled files on demand.
///     One instance is created per kernel build and caches fetched markdown for that turn.
/// </summary>
public class SkillToolset(
    IReadOnlyList<AgentSkill> skills,
    ISkillRegistryClient registryClient,
    ILogger? logger = null)
{
    private readonly Dictionary<Guid, string> _markdownCache = new();

    /// <summary>
    ///     Returns the catalog of skills attached to this agent (name — description).
    /// </summary>
    public string ListSkills()
    {
        if (skills.Count == 0)
        {
            return "No skills are attached to this agent.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Available skills (call load_skill with the exact name to read full instructions):");
        foreach (var skill in skills)
        {
            sb.AppendLine($"- {skill.DisplayName}: {Trim(skill.Description)}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Fetches the full SKILL.md instructions for one attached skill, by name.
    /// </summary>
    public async Task<string> LoadSkillAsync(string name, CancellationToken cancellationToken = default)
    {
        var skill = Match(name);
        if (skill is null)
        {
            return NotFoundMessage(name);
        }

        if (skill.Registry is null)
        {
            return $"Skill '{skill.DisplayName}' has no resolvable registry connection.";
        }

        if (_markdownCache.TryGetValue(skill.Id, out var cached))
        {
            return cached;
        }

        try
        {
            var markdown = await registryClient.GetSkillMarkdownAsync(
                skill.Registry, skill.Namespace, skill.SkillSlug, skill.Version, cancellationToken);
            _markdownCache[skill.Id] = markdown;
            return markdown;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to load skill {Skill}", skill.DisplayName);
            return $"Failed to load skill '{skill.DisplayName}': {ex.Message}";
        }
    }

    /// <summary>
    ///     Reads a bundled reference file from one attached skill, by name and relative path.
    /// </summary>
    public async Task<string> ReadSkillFileAsync(string name, string path, CancellationToken cancellationToken = default)
    {
        var skill = Match(name);
        if (skill is null)
        {
            return NotFoundMessage(name);
        }

        if (skill.Registry is null)
        {
            return $"Skill '{skill.DisplayName}' has no resolvable registry connection.";
        }

        try
        {
            return await registryClient.ReadFileAsync(
                skill.Registry, skill.Namespace, skill.SkillSlug, skill.Version, path, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to read file {Path} from skill {Skill}", path, skill.DisplayName);
            return $"Failed to read '{path}' from skill '{skill.DisplayName}': {ex.Message}";
        }
    }

    private AgentSkill? Match(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var needle = name.Trim();
        return skills.FirstOrDefault(s =>
                   s.DisplayName.Equals(needle, StringComparison.OrdinalIgnoreCase)
                   || s.SkillSlug.Equals(needle, StringComparison.OrdinalIgnoreCase)
                   || $"{s.Namespace}/{s.SkillSlug}".Equals(needle, StringComparison.OrdinalIgnoreCase));
    }

    private string NotFoundMessage(string name)
    {
        var names = string.Join(", ", skills.Select(s => s.DisplayName));
        return $"No attached skill named '{name}'. Attached skills: {names}.";
    }

    private static string Trim(string value) =>
        value.Length <= 200 ? value : value[..200] + "…";
}
