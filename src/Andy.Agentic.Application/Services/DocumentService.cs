using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service implementation for document operations.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAgentDocumentRepository _agentDocumentRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentService"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="agentDocumentRepository">The agent-document repository.</param>
    public DocumentService(
        IDocumentRepository documentRepository,
        IAgentDocumentRepository agentDocumentRepository)
    {
        _documentRepository = documentRepository;
        _agentDocumentRepository = agentDocumentRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _documentRepository.GetAllAsync();
        return documents.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        return document != null ? MapToDto(document) : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByAgentIdAsync(Guid agentId)
    {
        var agentDocuments = await _agentDocumentRepository.GetByAgentIdAsync(agentId);
        return agentDocuments.Select(ad => MapToDto(ad.Document));
    }

    /// <inheritdoc />
    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto, Guid? userId = null)
    {
        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            Name = createDocumentDto.Name,
            Description = createDocumentDto.Description,
            Type = createDocumentDto.Type,
            Content = createDocumentDto.Content,
            BinaryContent = createDocumentDto.BinaryContent,
            FilePath = null, // Not using file paths
            Size = createDocumentDto.Size,
            IsActive = createDocumentDto.IsActive,
            IsPublic = createDocumentDto.IsPublic,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdDocument = await _documentRepository.AddAsync(document);
        return MapToDto(createdDocument);
    }

    /// <inheritdoc />
    public async Task<DocumentUploadResponseDto> UploadDocumentAsync(IFormFile? file, Guid agentId, Guid? userId = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new DocumentUploadResponseDto
                {
                    Success = false,
                    Message = "No file provided",
                    Error = "File is empty or null"
                };
            }

            byte[] binaryContent;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                binaryContent = memoryStream.ToArray();
            }

            string? textContent = null;
            var fileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
            var textFileTypes = new[] { "txt", "md", "json", "xml", "html", "css", "js", "csv" };
            
            if (textFileTypes.Contains(fileExtension))
            {
                textContent = Encoding.UTF8.GetString(binaryContent);
            }

            var document = new DocumentEntity
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                Type = fileExtension,
                Content = textContent,
                BinaryContent = binaryContent,
                FilePath = null, // Not using file paths
                Size = file.Length,
                IsActive = true,
                IsPublic = false,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdDocument = await _documentRepository.AddAsync(document);

            // Associate with agent
            await AssociateDocumentWithAgentAsync(agentId, createdDocument.Id);

            return new DocumentUploadResponseDto
            {
                Success = true,
                Message = "Document uploaded successfully",
                Document = MapToDto(createdDocument)
            };
        }
        catch (Exception ex)
        {
            return new DocumentUploadResponseDto
            {
                Success = false,
                Message = "Failed to upload document",
                Error = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<byte[]?> DownloadDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null)
        {
            return null;
        }

        // Return binary content if available, otherwise convert text content to bytes
        if (document.BinaryContent != null && document.BinaryContent.Length > 0)
        {
            return document.BinaryContent;
        }

        if (!string.IsNullOrEmpty(document.Content))
        {
            return Encoding.UTF8.GetBytes(document.Content);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null)
        {
            return false;
        }

        // Remove all agent associations
        var agentDocuments = await _agentDocumentRepository.GetByDocumentIdAsync(id);
        foreach (var agentDocument in agentDocuments)
        {
            await _agentDocumentRepository.DeleteAsync(agentDocument.Id);
        }

        // Delete document
        return await _documentRepository.DeleteAsync(id);
    }

    /// <inheritdoc />
    public async Task<bool> AssociateDocumentWithAgentAsync(Guid agentId, Guid documentId)
    {
        // Check if association already exists
        if (await _agentDocumentRepository.ExistsAsync(agentId, documentId))
        {
            return true;
        }

        var agentDocument = new AgentDocumentEntity
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            DocumentId = documentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _agentDocumentRepository.AddAsync(agentDocument);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveDocumentFromAgentAsync(Guid agentId, Guid documentId)
    {
        return await _agentDocumentRepository.DeleteByAgentAndDocumentIdAsync(agentId, documentId);
    }

    /// <summary>
    ///     Maps a DocumentEntity to a DocumentDto.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <returns>The document DTO.</returns>
    private static DocumentDto MapToDto(DocumentEntity entity)
    {
        return new DocumentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type,
            Content = entity.Content,
            BinaryContent = entity.BinaryContent,
            FilePath = entity.FilePath,
            Size = entity.Size,
            IsActive = entity.IsActive,
            IsPublic = entity.IsPublic,
            IsRagProcessed = entity.IsRagProcessed,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedByUserId = entity.CreatedByUserId
        };
    }
}
