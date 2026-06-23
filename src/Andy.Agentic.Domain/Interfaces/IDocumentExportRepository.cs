using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces;

/// <summary>
///     Repository interface for DocumentExport entities.
/// </summary>
public interface IDocumentExportRepository
{
    /// <summary>
    ///     Gets a document export by its ID.
    /// </summary>
    /// <param name="id">The document export ID.</param>
    /// <returns>The document export if found, null otherwise.</returns>
    Task<DocumentExportEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Gets a document export by its file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The document export if found, null otherwise.</returns>
    Task<DocumentExportEntity?> GetByFileNameAsync(string fileName);

    /// <summary>
    ///     Gets all document exports.
    /// </summary>
    /// <returns>A collection of all document exports.</returns>
    Task<IEnumerable<DocumentExportEntity>> GetAllAsync();

    /// <summary>
    ///     Gets document exports by agent ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A collection of document exports associated with the agent.</returns>
    Task<IEnumerable<DocumentExportEntity>> GetByAgentIdAsync(Guid agentId);

    /// <summary>
    ///     Gets document exports by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of document exports created by the user.</returns>
    Task<IEnumerable<DocumentExportEntity>> GetByUserIdAsync(Guid userId);

    /// <summary>
    ///     Adds a new document export.
    /// </summary>
    /// <param name="documentExport">The document export to add.</param>
    /// <returns>The added document export.</returns>
    Task<DocumentExportEntity> AddAsync(DocumentExportEntity documentExport);

    /// <summary>
    ///     Deletes a document export by its ID.
    /// </summary>
    /// <param name="id">The document export ID.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Checks if a document export exists.
    /// </summary>
    /// <param name="id">The document export ID.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Guid id);
}

