using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
///     Repository interface for Agent-Document relationships.
/// </summary>
public interface IAgentDocumentRepository
{
    /// <summary>
    ///     Gets all agent-document relationships.
    /// </summary>
    /// <returns>A collection of all agent-document relationships.</returns>
    Task<IEnumerable<AgentDocumentEntity>> GetAllAsync();

    /// <summary>
    ///     Gets agent-document relationships by agent ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A collection of agent-document relationships for the agent.</returns>
    Task<IEnumerable<AgentDocumentEntity>> GetByAgentIdAsync(Guid agentId);

    /// <summary>
    ///     Gets agent-document relationships by document ID.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <returns>A collection of agent-document relationships for the document.</returns>
    Task<IEnumerable<AgentDocumentEntity>> GetByDocumentIdAsync(Guid documentId);

    /// <summary>
    ///     Gets a specific agent-document relationship.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>The agent-document relationship if found, null otherwise.</returns>
    Task<AgentDocumentEntity?> GetByAgentAndDocumentIdAsync(Guid agentId, Guid documentId);

    /// <summary>
    ///     Adds a new agent-document relationship.
    /// </summary>
    /// <param name="agentDocument">The agent-document relationship to add.</param>
    /// <returns>The added agent-document relationship.</returns>
    Task<AgentDocumentEntity> AddAsync(AgentDocumentEntity agentDocument);

    /// <summary>
    ///     Deletes an agent-document relationship by its ID.
    /// </summary>
    /// <param name="id">The relationship ID.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Deletes an agent-document relationship by agent and document IDs.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteByAgentAndDocumentIdAsync(Guid agentId, Guid documentId);

    /// <summary>
    ///     Checks if an agent-document relationship exists.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Guid agentId, Guid documentId);
}
