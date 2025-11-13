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
    private readonly string _exportDirectory;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public ExportsController(ILogger<ExportsController> logger)
    {
        _logger = logger;
        _exportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "exports");
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    /// <summary>
    /// Downloads an exported document
    /// </summary>
    /// <param name="fileName">The filename of the exported document</param>
    /// <returns>The file content</returns>
    [HttpGet("{fileName}")]
    public async Task<IActionResult> DownloadExport(string fileName)
    {
        try
        {
            // Validate filename to prevent directory traversal attacks
            if (string.IsNullOrWhiteSpace(fileName) || 
                fileName.Contains("..") || 
                fileName.Contains("/") || 
                fileName.Contains("\\"))
            {
                _logger.LogWarning("Invalid filename requested: {FileName}", fileName);
                return BadRequest("Invalid filename");
            }

            var filePath = Path.Combine(_exportDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Export file not found: {FilePath}", filePath);
                return NotFound("Exported document not found");
            }

            // Get the content type
            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // Read the file
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            _logger.LogInformation("Serving export file: {FileName}, Size: {Size} bytes", fileName, fileBytes.Length);

            // Return the file with proper content disposition for download
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving export file: {FileName}", fileName);
            return StatusCode(500, "An error occurred while downloading the exported document");
        }
    }

    /// <summary>
    /// Cleans up old exported documents (optional maintenance endpoint)
    /// </summary>
    /// <param name="olderThanHours">Delete files older than specified hours (default: 24)</param>
    /// <returns>Number of files deleted</returns>
    [HttpDelete("cleanup")]
    public IActionResult CleanupOldExports([FromQuery] int olderThanHours = 24)
    {
        try
        {
            if (!Directory.Exists(_exportDirectory))
            {
                return Ok(new { deletedCount = 0, message = "Export directory does not exist" });
            }

            var cutoffTime = DateTime.UtcNow.AddHours(-olderThanHours);
            var files = Directory.GetFiles(_exportDirectory);
            var deletedCount = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTimeUtc < cutoffTime)
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                    _logger.LogInformation("Deleted old export file: {FileName}", fileInfo.Name);
                }
            }

            _logger.LogInformation("Cleanup completed: {Count} files deleted", deletedCount);
            return Ok(new { deletedCount, message = $"Deleted {deletedCount} files older than {olderThanHours} hours" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during export cleanup");
            return StatusCode(500, "An error occurred during cleanup");
        }
    }
}

