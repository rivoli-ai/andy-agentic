using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

/// <summary>
///     Application service for managing skill registry connections and per-agent skill
///     attachments, and for proxying registry search so the SPA never holds registry credentials.
/// </summary>
public interface ISkillManager
{
    /// <summary>
    ///     Lists all configured skill registry connections.
    /// </summary>
    Task<IEnumerable<SkillRegistry>> GetRegistriesAsync();

    /// <summary>
    ///     Gets a single registry connection by id.
    /// </summary>
    Task<SkillRegistry?> GetRegistryAsync(Guid id);

    /// <summary>
    ///     Creates a new registry connection.
    /// </summary>
    Task<SkillRegistry> CreateRegistryAsync(SkillRegistry registry);

    /// <summary>
    ///     Updates an existing registry connection.
    /// </summary>
    Task<SkillRegistry> UpdateRegistryAsync(SkillRegistry registry);

    /// <summary>
    ///     Deletes a registry connection.
    /// </summary>
    Task<bool> DeleteRegistryAsync(Guid id);

    /// <summary>
    ///     Tests connectivity to a registry connection.
    /// </summary>
    Task<bool> TestRegistryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches a registry connection for skills matching the query.
    /// </summary>
    Task<IReadOnlyList<SkillSearchResult>> SearchAsync(Guid registryId, string query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the skills attached to an agent.
    /// </summary>
    Task<IEnumerable<AgentSkill>> GetAgentSkillsAsync(Guid agentId);

    /// <summary>
    ///     Attaches a skill (namespace/skill@version) from a registry to an agent.
    /// </summary>
    Task<AgentSkill> AttachSkillAsync(Guid agentId, AgentSkill skill);

    /// <summary>
    ///     Detaches a skill from an agent.
    /// </summary>
    Task<bool> DetachSkillAsync(Guid agentId, Guid agentSkillId);
}
