using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service for managing agent operations including CRUD operations, search, and filtering.
///     Acts as a facade over the database service for agent-related functionality.
/// </summary>
public class AgentService(IDataBaseService databaseResourceAccess, IMapper mapper) : IAgentService
{
    /// <summary>
    ///     Retrieves all agents from the database.
    /// </summary>
    /// <returns>A collection of all agent s.</returns>
    public async Task<IEnumerable<Agent>> GetAllAgentsAsync() => await databaseResourceAccess.GetAllAgentsAsync();

    /// <summary>
    ///     Retrieves a specific agent by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the agent.</param>
    /// <returns>The agent  if found; otherwise, null.</returns>
    public async Task<Agent?> GetAgentByIdAsync(Guid id) => await databaseResourceAccess.GetAgentByIdAsync(id);

    /// <summary>
    ///     Creates a new agent in the database.
    /// </summary>
    /// <param name="createAgent">The agent data for creation.</param>
    /// <returns>The created agent .</returns>
    public async Task<Agent> CreateAgentAsync(Agent createAgent) => await databaseResourceAccess.CreateAgentAsync(mapper.Map<Agent>(createAgent));

    /// <summary>
    ///     Updates an existing agent in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the agent to update.</param>
    /// <param name="updateAgent">The updated agent data.</param>
    /// <returns>The updated agent .</returns>
    public async Task<Agent> UpdateAgentAsync(Agent updateAgent) =>
        await databaseResourceAccess.UpdateAgentAsync(updateAgent);

    /// <summary>
    ///     Deletes an agent from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the agent to delete.</param>
    /// <returns>True if the agent was successfully deleted; false if not found.</returns>
    public async Task<bool> DeleteAgentAsync(Guid id) => await databaseResourceAccess.DeleteAgentAsync(id);

    /// <summary>
    ///     Searches for agents using a free-text query.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A collection of agent s matching the search criteria.</returns>
    public async Task<IEnumerable<Agent>> SearchAgentsAsync(string query) =>
        await databaseResourceAccess.SearchAgentsAsync(query);

    /// <summary>
    ///     Retrieves agents filtered by their type.
    /// </summary>
    /// <param name="type">The agent type to filter by.</param>
    /// <returns>A collection of agent s of the specified type.</returns>
    public async Task<IEnumerable<Agent>> GetAgentsByTypeAsync(string type) =>
        await databaseResourceAccess.GetAgentsByTypeAsync(type);

    /// <summary>
    ///     Retrieves agents filtered by a specific tag.
    /// </summary>
    /// <param name="tag">The tag to filter agents by.</param>
    /// <returns>A collection of agent s associated with the specified tag.</returns>
    public async Task<IEnumerable<Agent>> GetAgentsByTagAsync(string tag) =>
        await databaseResourceAccess.GetAgentsByTagAsync(tag);
}
