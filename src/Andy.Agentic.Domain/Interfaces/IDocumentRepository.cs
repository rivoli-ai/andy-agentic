using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
///     Repository interface for Document entities.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    ///     Gets all documents.
    /// </summary>
    /// <returns>A collection of all documents.</returns>
    Task<IEnumerable<DocumentEntity>> GetAllAsync();

    /// <summary>
    ///     Gets a document by its ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>The document if found, null otherwise.</returns>
    Task<DocumentEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Gets documents by agent ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A collection of documents associated with the agent.</returns>
    Task<IEnumerable<DocumentEntity>> GetByAgentIdAsync(Guid agentId);

    /// <summary>
    ///     Adds a new document.
    /// </summary>
    /// <param name="document">The document to add.</param>
    /// <returns>The added document.</returns>
    Task<DocumentEntity> AddAsync(DocumentEntity document);

    /// <summary>
    ///     Updates an existing document.
    /// </summary>
    /// <param name="document">The document to update.</param>
    /// <returns>The updated document.</returns>
    Task<DocumentEntity> UpdateAsync(DocumentEntity document);

    /// <summary>
    ///     Deletes a document by its ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Checks if a document exists.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Guid id);
}
