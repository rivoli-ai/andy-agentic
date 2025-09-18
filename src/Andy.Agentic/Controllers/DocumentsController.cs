using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     Controller for document operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentRagProvider _documentRagService;
    private readonly IAgentService _agentService;
    private readonly IDocumentRagBackgroundService _documentRagBackgroundService;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentsController"/> class.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    /// <param name="documentRagService">The document RAG service.</param>
    /// <param name="agentService">The agent service.</param>
    /// <param name="documentRagBackgroundService">The document RAG background service.</param>
    /// <param name="logger">The logger.</param>
    public DocumentsController(
        IDocumentService documentService, 
        IDocumentRagProvider documentRagService,
        IAgentService agentService,
        IDocumentRagBackgroundService documentRagBackgroundService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _documentRagService = documentRagService;
        _agentService = agentService;
        _documentRagBackgroundService = documentRagBackgroundService;
        _logger = logger;
    }

    /// <summary>
    ///     Gets all documents.
    /// </summary>
    /// <returns>A collection of all documents.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
    {
        try
        {
            var documents = await _documentService.GetAllDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            return StatusCode(500, "An error occurred while retrieving documents");
        }
    }

    /// <summary>
    ///     Gets a document by its ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>The document if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
            return StatusCode(500, "An error occurred while retrieving the document");
        }
    }

    /// <summary>
    ///     Gets documents by agent ID.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A collection of documents associated with the agent.</returns>
    [HttpGet("agent/{agentId}")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByAgentId(Guid agentId)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByAgentIdAsync(agentId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for agent {AgentId}", agentId);
            return StatusCode(500, "An error occurred while retrieving documents for the agent");
        }
    }

    /// <summary>
    ///     Creates a new document.
    /// </summary>
    /// <param name="createDocumentDto">The document creation data.</param>
    /// <returns>The created document.</returns>
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentDto createDocumentDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var document = await _documentService.CreateDocumentAsync(createDocumentDto);
            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, "An error occurred while creating the document");
        }
    }

    /// <summary>
    ///     Uploads a document file.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="agentId">The agent ID to associate with the document.</param>
    /// <returns>The upload response.</returns>
    [HttpPost("upload")]
    public async Task<ActionResult<DocumentUploadResponseDto>> UploadDocument(IFormFile file, [FromForm] Guid agentId)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            var result = await _documentService.UploadDocumentAsync(file, agentId);
            
            if (!result.Success)
                return BadRequest(result);

            // Queue document for RAG processing
            try
            {
                await _documentRagBackgroundService.QueueDocumentForProcessingAsync(result.Document?.Id, agentId);
                _logger.LogInformation("Queued document {DocumentId} for RAG processing with agent {AgentId}",
                    result.Document?.Id, agentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not queue document {DocumentId} for RAG processing - continuing with upload", result.Document?.Id);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for agent {AgentId}", agentId);
            return StatusCode(500, "An error occurred while uploading the document");
        }
    }

    /// <summary>
    ///     Downloads a document file.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>The document file.</returns>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            var fileBytes = await _documentService.DownloadDocumentAsync(id);
            if (fileBytes == null)
                return NotFound("Document file not found");

            return File(fileBytes, GetContentType(document.Type), document.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, "An error occurred while downloading the document");
        }
    }

    /// <summary>
    ///     Deletes a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var success = await _documentService.DeleteDocumentAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return StatusCode(500, "An error occurred while deleting the document");
        }
    }

    /// <summary>
    ///     Associates a document with an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("associate")]
    public async Task<IActionResult> AssociateDocumentWithAgent([FromQuery] Guid agentId, [FromQuery] Guid documentId)
    {
        try
        {
            var success = await _documentService.AssociateDocumentWithAgentAsync(agentId, documentId);
            if (!success)
                return BadRequest("Failed to associate document with agent");

            // Queue document for RAG processing
            try
            {
                await _documentRagBackgroundService.QueueDocumentForProcessingAsync(documentId, agentId);
                _logger.LogInformation("Queued associated document {DocumentId} for RAG processing with agent {AgentId}", 
                    documentId, agentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not queue associated document {DocumentId} for RAG processing - continuing with association", documentId);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating document {DocumentId} with agent {AgentId}", documentId, agentId);
            return StatusCode(500, "An error occurred while associating the document with the agent");
        }
    }

    /// <summary>
    ///     Removes the association between a document and an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("associate")]
    public async Task<IActionResult> RemoveDocumentFromAgent([FromQuery] Guid agentId, [FromQuery] Guid documentId)
    {
        try
        {
            var success = await _documentService.RemoveDocumentFromAgentAsync(agentId, documentId);
            if (!success)
                return BadRequest("Failed to remove document from agent");

            // Remove document from RAG processing if agent has embedding configuration
            try
            {
                var agent = await _agentService.GetAgentByIdAsync(agentId);
                if (agent != null && agent.EmbeddingLlmConfig != null)
                {
                    await _documentRagService.RemoveDocumentFromRagAsync(documentId, agent);
                    _logger.LogInformation("Successfully removed document {DocumentId} from RAG processing for agent {AgentId}", 
                        documentId, agentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not remove document {DocumentId} from RAG processing - continuing with removal", documentId);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing document {DocumentId} from agent {AgentId}", documentId, agentId);
            return StatusCode(500, "An error occurred while removing the document from the agent");
        }
    }

    /// <summary>
    ///     Processes all documents for an agent for RAG.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("process-rag/{agentId}")]
    public async Task<IActionResult> ProcessDocumentsForRag(Guid agentId)
    {
        try
        {
            var agent = await _agentService.GetAgentByIdAsync(agentId);
            if (agent == null)
                return NotFound("Agent not found");

            if (agent.EmbeddingLlmConfig == null)
                return BadRequest("Agent does not have an embedding configuration");

            // Process all documents for RAG asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await _documentRagService.ProcessAllDocumentsForAgentAsync(agent);
                    _logger.LogInformation("Successfully processed all documents for RAG with agent {AgentId}", agentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing all documents for RAG with agent {AgentId}", agentId);
                }
            });

            return Ok(new { message = "Document processing started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting document processing for agent {AgentId}", agentId);
            return StatusCode(500, "An error occurred while starting document processing");
        }
    }

    /// <summary>
    ///     Gets the status of the document RAG background service.
    /// </summary>
    /// <returns>The service status.</returns>
    [HttpGet("rag-status")]
    public IActionResult GetRagServiceStatus()
    {
        try
        {
            var status = new
            {
                IsRunning = _documentRagBackgroundService.IsRunning,
                QueueCount = _documentRagBackgroundService.QueueCount,
                Timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting RAG service status");
            return StatusCode(500, "An error occurred while getting RAG service status");
        }
    }

    /// <summary>
    ///     Tests the document RAG background service by queuing a test item.
    /// </summary>
    /// <returns>The test result.</returns>
    [HttpPost("test-rag-queue")]
    public async Task<IActionResult> TestRagQueue()
    {
        try
        {
            _logger.LogInformation("Testing RAG queue with test document");
            await _documentRagBackgroundService.QueueDocumentForProcessingAsync(Guid.NewGuid(), Guid.NewGuid());
            return Ok(new { message = "Test item queued successfully", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing RAG queue");
            return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    ///     Checks if the background service is properly registered and accessible.
    /// </summary>
    /// <returns>The service registration status.</returns>
    [HttpGet("check-service-registration")]
    public IActionResult CheckServiceRegistration()
    {
        try
        {
            var serviceType = _documentRagBackgroundService.GetType();
            var isRunning = _documentRagBackgroundService.IsRunning;
            
            return Ok(new 
            { 
                message = "Service is registered and accessible",
                serviceType = serviceType.FullName,
                isRunning = isRunning,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service registration");
            return BadRequest(new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    ///     Gets the content type for a file extension.
    /// </summary>
    /// <param name="fileType">The file type.</param>
    /// <returns>The content type.</returns>
    private static string GetContentType(string fileType)
    {
        return fileType.ToLower() switch
        {
            "pdf" => "application/pdf",
            "txt" => "text/plain",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "csv" => "text/csv",
            "json" => "application/json",
            "xml" => "application/xml",
            "html" => "text/html",
            "md" => "text/markdown",
            _ => "application/octet-stream"
        };
    }
}
