using Andy.Agentic.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Andy.Agentic.Application.Interfaces;

/// <summary>
///     Service interface for document operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    ///     Gets all documents.
    /// </summary>
    /// <returns>A collection of all documents.</returns>
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();

    /// <summary>
    ///     Gets a document by its ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>The document if found, null otherwise.</returns>
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id);

    /// <summary>
    ///     Gets documents by agent ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A collection of documents associated with the agent.</returns>
    Task<IEnumerable<DocumentDto>> GetDocumentsByAgentIdAsync(Guid agentId);

    /// <summary>
    ///     Creates a new document.
    /// </summary>
    /// <param name="createDocumentDto">The document creation data.</param>
    /// <param name="userId">The ID of the user creating the document.</param>
    /// <returns>The created document.</returns>
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto, Guid? userId = null);

    /// <summary>
    ///     Uploads a document file.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="agentId">The agent ID to associate with the document.</param>
    /// <param name="userId">The ID of the user uploading the document.</param>
    /// <returns>The upload response.</returns>
    Task<DocumentUploadResponseDto> UploadDocumentAsync(IFormFile file, Guid agentId, Guid? userId = null);

    /// <summary>
    ///     Downloads a document file.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>The document file as a byte array.</returns>
    Task<byte[]?> DownloadDocumentAsync(Guid id);

    /// <summary>
    ///     Deletes a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteDocumentAsync(Guid id);

    /// <summary>
    ///     Associates a document with an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>True if associated, false otherwise.</returns>
    Task<bool> AssociateDocumentWithAgentAsync(Guid agentId, Guid documentId);

    /// <summary>
    ///     Removes the association between a document and an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>True if removed, false otherwise.</returns>
    Task<bool> RemoveDocumentFromAgentAsync(Guid agentId, Guid documentId);
}
