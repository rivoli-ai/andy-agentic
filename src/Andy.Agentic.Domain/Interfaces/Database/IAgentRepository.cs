using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides data access operations for <see cref="AgentEntity" /> instances.
/// </summary>
public interface IAgentRepository : IBaseRepository<AgentEntity>

{
    /// <summary>
    ///     Retrieves all agents.
    /// </summary>
    Task<IEnumerable<AgentEntity>> GetAllAsync();

    /// <summary>
    ///     Retrieves an agent by its identifier.
    /// </summary>
    Task<AgentEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Creates a new agent.
    /// </summary>
    Task<AgentEntity> CreateAsync(AgentEntity agent);

    /// <summary>
    ///     Updates an existing agent.
    /// </summary>
    Task<AgentEntity> UpdateAsync(AgentEntity agent);

    /// <summary>
    ///     Deletes an agent by identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Searches agents by a free-text query.
    /// </summary>
    Task<IEnumerable<AgentEntity>> SearchAsync(string query);

    /// <summary>
    ///     Gets agents matching the specified type.
    /// </summary>
    Task<IEnumerable<AgentEntity>> GetByTypeAsync(string type);

    /// <summary>
    ///     Gets agents associated with the specified tag.
    /// </summary>
    Task<IEnumerable<AgentEntity>> GetByTagAsync(string tag);

    /// <summary>
    ///     Retrieves an agent by name.
    /// </summary>
    Task<AgentEntity?> GetByNameAsync(string name);

    /// <summary>
    ///     Retrieves an agent including its LLM configuration.
    /// </summary>
    Task<AgentEntity?> GetWithConfigAsync(Guid id);

    /// <summary>
    ///     Retrieves agents visible to the specified user.
    ///     Returns public agents and agents created by the user.
    /// </summary>
    Task<IEnumerable<AgentEntity>> GetVisibleAsync(Guid userId);

    /// <summary>
    ///     Retrieves a specific agent by ID if it's visible to the specified user.
    /// </summary>
    Task<AgentEntity?> GetVisibleByIdAsync(Guid id, Guid userId);
}
