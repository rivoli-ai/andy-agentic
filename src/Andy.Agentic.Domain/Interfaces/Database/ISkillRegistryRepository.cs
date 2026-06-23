using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides data access operations for <see cref="SkillRegistryEntity" /> connections
///     and the <see cref="AgentSkillEntity" /> agent attachments.
/// </summary>
public interface ISkillRegistryRepository
{
    /// <summary>
    ///     Retrieves all registry connections.
    /// </summary>
    Task<IEnumerable<SkillRegistryEntity>> GetAllAsync();

    /// <summary>
    ///     Retrieves a registry connection by identifier.
    /// </summary>
    Task<SkillRegistryEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Creates a new registry connection.
    /// </summary>
    Task<SkillRegistryEntity> CreateAsync(SkillRegistryEntity registry);

    /// <summary>
    ///     Updates an existing registry connection.
    /// </summary>
    Task<SkillRegistryEntity> UpdateAsync(SkillRegistryEntity registry);

    /// <summary>
    ///     Deletes a registry connection by identifier. Fails if skills from it are attached to agents.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Lists the skills attached to an agent (with their registry connection loaded).
    /// </summary>
    Task<IEnumerable<AgentSkillEntity>> GetAgentSkillsAsync(Guid agentId);

    /// <summary>
    ///     Attaches a skill to an agent. Returns the created association.
    /// </summary>
    Task<AgentSkillEntity> AttachSkillAsync(AgentSkillEntity agentSkill);

    /// <summary>
    ///     Detaches a skill from an agent. Returns false if the association does not exist.
    /// </summary>
    Task<bool> DetachSkillAsync(Guid agentId, Guid agentSkillId);
}
