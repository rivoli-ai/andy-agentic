using Andy.Agentic.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Andy.Agentic.Controllers;

/// <summary>
/// Controller for serving exported documents
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExportsController : ControllerBase
{
    private readonly ILogger<ExportsController> _logger;
    private readonly IDocumentExportRepository _documentExportRepository;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public ExportsController(
        ILogger<ExportsController> logger,
        IDocumentExportRepository documentExportRepository)
    {
        _logger = logger;
        _documentExportRepository = documentExportRepository;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    /// <summary>
    /// Downloads an exported document by ID
    /// </summary>
    /// <param name="id">The ID of the exported document</param>
    /// <returns>The file content</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadExport(Guid id)
    {
        try
        {
            var documentExport = await _documentExportRepository.GetByIdAsync(id);
            
            if (documentExport == null)
            {
                _logger.LogWarning("Export document not found: {Id}", id);
                return NotFound("Exported document not found");
            }

            // Get the content type based on file extension
            if (!_contentTypeProvider.TryGetContentType(documentExport.FileName, out var contentType))
            {
                // Default content types based on format
                contentType = documentExport.Format.ToLowerInvariant() switch
                {
                    "excel" or "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "pdf" => "application/pdf",
                    "word" or "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };
            }

            _logger.LogInformation("Serving export file: {FileName}, Size: {Size} bytes, ID: {Id}", 
                documentExport.FileName, documentExport.Size, id);

            // Return the file with proper content disposition for download
            return File(documentExport.Content, contentType, documentExport.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving export file: {Id}", id);
            return StatusCode(500, "An error occurred while downloading the exported document");
        }
    }

    /// <summary>
    /// Cleans up old exported documents (optional maintenance endpoint)
    /// </summary>
    /// <param name="olderThanHours">Delete documents older than specified hours (default: 24)</param>
    /// <returns>Number of documents deleted</returns>
    [HttpDelete("cleanup")]
    public async Task<IActionResult> CleanupOldExports([FromQuery] int olderThanHours = 24)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-olderThanHours);
            var allExports = await _documentExportRepository.GetAllAsync();
            var oldExports = allExports.Where(e => e.CreatedAt < cutoffTime).ToList();
            var deletedCount = 0;

            foreach (var export in oldExports)
            {
                var deleted = await _documentExportRepository.DeleteAsync(export.Id);
                if (deleted)
                {
                    deletedCount++;
                    _logger.LogInformation("Deleted old export: {FileName}, ID: {Id}", export.FileName, export.Id);
                }
            }

            _logger.LogInformation("Cleanup completed: {Count} documents deleted", deletedCount);
            return Ok(new { deletedCount, message = $"Deleted {deletedCount} documents older than {olderThanHours} hours" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during export cleanup");
            return StatusCode(500, "An error occurred during cleanup");
        }
    }
}

