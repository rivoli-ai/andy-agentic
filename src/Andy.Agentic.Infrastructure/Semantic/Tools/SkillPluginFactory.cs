using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Andy.Agentic.Infrastructure.Semantic.Tools;

/// <summary>
///     Builds and imports the per-agent <c>skills</c> plugin into a Semantic Kernel. The plugin
///     exposes three native functions implementing progressive disclosure over skills attached
///     to the agent from a configured registry.
/// </summary>
public class SkillPluginFactory(ISkillRegistryClient registryClient, ILogger<SkillPluginFactory>? logger = null)
{
    /// <summary>
    ///     Imports the <c>skills</c> plugin into <paramref name="kernel"/> for the given agent,
    ///     if the agent has any active skills attached. No-op otherwise.
    /// </summary>
    public void AddSkills(Kernel kernel, Agent agent)
    {
        var skills = agent.Skills.Where(s => s.IsActive).ToList();
        if (skills.Count == 0)
        {
            return;
        }

        var toolset = new SkillToolset(skills, registryClient, logger);

#pragma warning disable SKEXP0130
        var functions = new[]
        {
            KernelFunctionFactory.CreateFromMethod(
                method: () => toolset.ListSkills(),
                functionName: "list_skills",
                description: "List the skills available to this agent (name and short description). " +
                             "Call this first to discover what procedural skills you can use."),

            KernelFunctionFactory.CreateFromMethod(
                method: async (string name) => await toolset.LoadSkillAsync(name),
                functionName: "load_skill",
                description: "Load the full instructions (SKILL.md) for one skill by its exact name. " +
                             "Call this when a skill from list_skills is relevant to the task, then follow its instructions."),

            KernelFunctionFactory.CreateFromMethod(
                method: async (string name, string path, int offset, int limit) =>
                    await toolset.ReadSkillFileAsync(name, path, offset, limit),
                functionName: "read_skill_file",
                description: "Read a bundled reference file from a skill, by skill name and relative file path " +
                             "(paths come from the skill's instructions). Large files are paged: pass offset/limit, " +
                             "and if the result says more remains, call again with the suggested offset."),
        };

        kernel.ImportPluginFromFunctions("skills", functions);
#pragma warning restore SKEXP0130
        logger?.LogInformation("Imported skills plugin with {Count} attached skill(s) for agent {AgentId}", skills.Count, agent.Id);
    }

    /// <summary>
    ///     Returns a short system-prompt addendum announcing the attached skills, or empty if none.
    /// </summary>
    public static string BuildCatalogPrompt(Agent agent)
    {
        var skills = agent.Skills.Where(s => s.IsActive).ToList();
        if (skills.Count == 0)
        {
            return string.Empty;
        }

        var names = string.Join(", ", skills.Select(s => s.DisplayName));
        return $"You have access to the following skills via the skills plugin: {names}. " +
               "Use list_skills to review them and load_skill to read a skill's full instructions before acting on it.";
    }
}
