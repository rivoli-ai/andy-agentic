namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;

/// <summary>
/// Interface for document exporters
/// </summary>
public interface IDocumentExporter
{
    /// <summary>
    /// Export content to a document format
    /// </summary>
    /// <param name="content">The content to export</param>
    /// <param name="title">The title of the document</param>
    /// <returns>Byte array of the generated document</returns>
    Task<byte[]> ExportAsync(string content, string title);
    
    /// <summary>
    /// Gets the file extension for this exporter
    /// </summary>
    string FileExtension { get; }
    
    /// <summary>
    /// Gets the MIME type for this exporter
    /// </summary>
    string MimeType { get; }
}

