using System.Text;
using System.Text.Json;
using Andy.Agentic.Domain.Helpers;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Exposes attached skills to thinking models as on-demand function tools, implementing
///     Anthropic-style progressive disclosure:
///     <list type="number">
///         <item>only skill name + description are kept in the system prompt (cheap, always);</item>
///         <item><c>load_skill</c> returns the full SKILL.md when the model decides it is relevant;</item>
///         <item><c>read_skill_file</c> returns a bundled file the instructions reference.</item>
///     </list>
///     The Semantic Kernel path provides the same functions via its plugin; this is the equivalent
///     for the raw thinking-model path, executed inside its tool-call callback.
/// </summary>
public static class SkillToolCalling
{
    public const string LoadSkill = "load_skill";
    public const string ReadSkillFile = "read_skill_file";

    /// <summary>True if the function name is one of the skill tools.</summary>
    public static bool IsSkillTool(string? name) => name is LoadSkill or ReadSkillFile;

    /// <summary>OpenAI function schemas advertised to the model when the agent has skills.</summary>
    public static List<OpenAiTool> BuildToolSchemas() =>
    [
        new OpenAiTool
        {
            Type = "function",
            Function = new Function
            {
                Name = LoadSkill,
                Description = "Load the full instructions (SKILL.md) for one of the available skills, by its exact name. "
                              + "Call this first when a skill's description matches the user's request, then follow the returned instructions.",
                Parameters = new FunctionParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        ["skill"] = new() { Type = "string", Description = "Exact name of the skill to load." },
                    },
                    Required = ["skill"],
                },
            },
        },
        new OpenAiTool
        {
            Type = "function",
            Function = new Function
            {
                Name = ReadSkillFile,
                Description = "Read a bundled file referenced by a skill's instructions, by skill name and relative file path. "
                              + "Large files are returned in pages: if the result says more remains, call again with the "
                              + "suggested offset to read the next part.",
                Parameters = new FunctionParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        ["skill"] = new() { Type = "string", Description = "Exact name of the skill." },
                        ["path"] = new() { Type = "string", Description = "Relative path of the file inside the skill package." },
                        ["offset"] = new() { Type = "integer", Description = "Start character for paging (default 0)." },
                        ["limit"] = new() { Type = "integer", Description = "Max characters to return (default 40000)." },
                    },
                    Required = ["skill", "path"],
                },
            },
        },
    ];

    /// <summary>
    ///     Builds the cheap Level-1 catalog (name + description only) injected into the system prompt.
    /// </summary>
    public static string? BuildCatalogPrompt(IReadOnlyList<AgentSkill> skills)
    {
        var active = skills.Where(s => s.IsActive).ToList();
        if (active.Count == 0)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.AppendLine("# Skills");
        sb.AppendLine("You can use the skills below. When a skill's description matches the user's request, FIRST call "
                      + "load_skill with its exact name to get the full instructions, then follow them. If those "
                      + "instructions reference a file, call read_skill_file with the skill name and the file path. "
                      + "Do not answer from assumptions when a skill clearly applies.");
        foreach (var s in active)
        {
            sb.AppendLine($"- {s.DisplayName}: {Trim(s.Description)}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    ///     Executes a <c>load_skill</c> / <c>read_skill_file</c> tool call against the registry.
    /// </summary>
    /// <returns>
    ///     The execution log (to feed back to the model) and, for a successful <c>load_skill</c>, the
    ///     display label of the loaded skill (used to populate the "skills used" indicator).
    /// </returns>
    public static async Task<(ToolExecutionLog Log, string? LoadedLabel)> ExecuteAsync(
        ToolCall call,
        IReadOnlyList<AgentSkill> skills,
        ISkillRegistryClient registryClient,
        Guid? agentId,
        string? sessionId,
        CancellationToken cancellationToken)
    {
        var args = ParseArgs(call.Function.Arguments);
        var name = FirstNonEmpty(args, "skill", "name", "skill_name", "skillName");

        ToolExecutionLog Log(bool ok, string result) => new()
        {
            Id = Guid.NewGuid(),
            ToolId = Guid.Empty, // synthetic: not a persisted Tool row (skills are not Tools)
            ToolName = call.Function.Name,
            AgentId = agentId,
            SessionId = sessionId,
            Parameters = args.ToDictionary(k => k.Key, k => (object)k.Value),
            Result = result,
            Success = ok,
            ExecutedAt = DateTime.UtcNow,
            Tool = new Tool { Name = call.Function.Name, Type = "skill" },
        };

        var skill = Match(skills, name);
        if (skill is null)
        {
            var available = string.Join(", ", skills.Select(s => s.DisplayName));
            return (Log(false, $"No attached skill named '{name}'. Available skills: {available}."), null);
        }

        if (skill.Registry is null)
        {
            return (Log(false, $"Skill '{skill.DisplayName}' has no resolvable registry connection."), null);
        }

        try
        {
            if (call.Function.Name == LoadSkill)
            {
                var markdown = await registryClient.GetSkillMarkdownAsync(
                    skill.Registry, skill.Namespace, skill.SkillSlug, skill.Version, cancellationToken);
                var label = $"{skill.DisplayName} ({skill.Namespace}/{skill.SkillSlug}@{skill.Version})";
                return (Log(true, markdown), label);
            }

            var path = FirstNonEmpty(args, "path", "file", "filePath", "file_path");
            if (string.IsNullOrWhiteSpace(path))
            {
                return (Log(false, "Missing 'path' argument for read_skill_file."), null);
            }

            var offset = ParseInt(args, "offset");
            var limit = ParseInt(args, "limit");
            var content = await registryClient.ReadFileAsync(
                skill.Registry, skill.Namespace, skill.SkillSlug, skill.Version, path, cancellationToken);
            return (Log(true, SkillContentWindow.Window(content, offset, limit, path)), null);
        }
        catch (Exception ex)
        {
            return (Log(false, $"Failed to execute {call.Function.Name} for '{skill.DisplayName}': {ex.Message}"), null);
        }
    }

    private static AgentSkill? Match(IReadOnlyList<AgentSkill> skills, string name)
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

    private static Dictionary<string, string> ParseArgs(string? json)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    result[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString() ?? string.Empty
                        : prop.Value.ToString();
                }
            }
        }
        catch (JsonException)
        {
            // tolerate malformed arguments
        }

        return result;
    }

    private static int ParseInt(Dictionary<string, string> args, string key)
    {
        if (args.TryGetValue(key, out var v) && int.TryParse(v, out var n))
        {
            return n;
        }

        return 0;
    }

    private static string FirstNonEmpty(Dictionary<string, string> args, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (args.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v))
            {
                return v;
            }
        }

        return string.Empty;
    }

    private static string Trim(string value) => value.Length <= 200 ? value : value[..200] + "…";
}
