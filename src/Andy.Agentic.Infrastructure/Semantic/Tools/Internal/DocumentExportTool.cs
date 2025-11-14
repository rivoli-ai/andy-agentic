using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Repositories;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json;
using Andy.Agentic.Domain.Interfaces.Database;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal;

/// <summary>
/// Tool that provides document export functionality for converting responses to various formats.
/// Supports exporting to Excel, PDF, and Word documents.
/// </summary>
public class  DocumentExportTool(
        DocumentExportFactory exportFactory,
        IDocumentExportRepository documentExportRepository,
        IDataBaseService databseService,
        ILogger<DocumentExportTool>? logger,
        IHttpContextAccessor? httpContextAccessor)
    {
      

    /// <summary>
    /// Exports content to the specified document format.
    /// </summary>
    /// <param name="content">The content to export</param>
    /// <param name="format">The document format (excel, pdf, or word)</param>
    /// <param name="title">The title of the document (optional, defaults to 'Exported Document')</param>
    /// <param name="toolConfig">The tool configuration containing apiUrl</param>
    /// <param name="agent">The agent executing the tool (optional)</param>
    /// <param name="sessionId">The session ID where the export was generated (optional)</param>
    /// <returns>A markdown formatted message with download link</returns>
    [Description("Exports content to a document format (excel, pdf, or word). Returns a markdown link for download.")]
    public async Task<string> ExportDocumentAsync(
        [Description("The content to export to the document")] string content,
        [Description("The document format: excel, pdf, or word")] string format,
        [Description("The title of the document")] string? title = null,
        Tool? toolConfig = null,
        Agent? agent = null,
        string? sessionId = null)
    {
        try
        {
            var documentTitle = string.IsNullOrWhiteSpace(title) ? "Exported Document" : title;
            
            // Extract apiUrl from tool configuration
            var apiUrl = "https://localhost"; // Default value
            if (toolConfig != null && !string.IsNullOrWhiteSpace(toolConfig.Configuration))
            {
                try
                {
                    var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(toolConfig.Configuration);
                    if (configDict != null && configDict.TryGetValue("apiUrl", out var url))
                    {
                        apiUrl = url;
                        logger?.LogInformation("Using API URL from tool configuration: {ApiUrl}", apiUrl);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Failed to parse tool configuration, using default API URL");
                }
            }
            
            logger?.LogInformation(
                "Exporting document - Format: {Format}, Title: {Title}, Content Length: {Length}, API URL: {ApiUrl}",
                format, documentTitle, content.Length, apiUrl);

            var exporter = exportFactory.GetExporter(format);
            var documentBytes = await exporter.ExportAsync(content, documentTitle);

            // Generate unique filename
            var fileName = $"{SanitizeFileName(documentTitle)}{Guid.NewGuid():N}{exporter.FileExtension}";


            var userId =await GetCurrentUserAsync();

            // Create document export entity
            var documentExport = new DocumentExportEntity
            {
                Title = documentTitle,
                Format = format.ToLowerInvariant(),
                FileName = fileName,
                Content = documentBytes,
                Size = documentBytes.Length,
                AgentId = agent?.Id,
                SessionId = sessionId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await documentExportRepository.AddAsync(documentExport);

            // Build full download URL using apiUrl from tool configuration
            // Use the document export ID instead of filename for better security
            var downloadUrl = $"{apiUrl}/api/exports/{documentExport.Id}";
            
            logger?.LogInformation(
                "Document exported successfully - Format: {Format}, Size: {Size} bytes, File: {FileName}, ID: {Id}, URL: {Url}",
                format, documentBytes.Length, fileName, documentExport.Id, downloadUrl);

            // Return markdown formatted message with download link
            var result = $"📄 **Document Export Complete**\n\n" +
                        $"**Title:** {documentTitle}\n" +
                        $"**Format:** {format.ToUpper()}\n" +
                        $"**Size:** {FormatFileSize(documentBytes.Length)}\n\n" +
                        $"**📥 Download:** [Click here to download {documentTitle}{exporter.FileExtension}]({downloadUrl})\n\n" +
                        $"The document has been exported successfully and is ready for download.";

            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error exporting document to format: {Format}", format);
            return $"Error occurred while exporting document: {ex.Message}\n" +
                   $"Supported formats are: {string.Join(", ", exportFactory.GetSupportedFormats())}";
        }
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters.
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(sanitized) ? "document" : sanitized;
    }

    /// <summary>
    /// Formats file size in human-readable format.
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Lists all supported document export formats.
    /// </summary>
    /// <returns>A string listing all supported formats</returns>
    [Description("Lists all supported document export formats")]
    public Task<string> ListSupportedFormatsAsync()
    {
        try
        {
            var formats = exportFactory.GetSupportedFormats();
            var result = "Supported Document Export Formats:\n" +
                        string.Join("\n", formats.Select(f => $"- {f.ToUpper()}"));
            
            logger?.LogInformation("Listed supported export formats");
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error listing supported formats");
            return Task.FromResult($"Error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    /// <returns>The current user DTO or null if not authenticated</returns>
    public async Task<Guid?> GetCurrentUserAsync()
    {
        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var azureAdId = httpContext.User.FindFirst("oid")?.Value ??
                        httpContext.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        if (string.IsNullOrEmpty(azureAdId))
        {
            return null;
        }

        var user = await databseService.GetUserIdByAzureId(azureAdId);
        return user;
    }

}

