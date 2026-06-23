using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;

/// <summary>
/// Factory for creating document exporters based on format type
/// </summary>
public class DocumentExportFactory
{
    private readonly ILogger<DocumentExportFactory>? _logger;
    private readonly Dictionary<string, IDocumentExporter> _exporters;

    public DocumentExportFactory(
        ExcelExporter excelExporter,
        PdfExporter pdfExporter,
        WordExporter wordExporter,
        ILogger<DocumentExportFactory>? logger = null)
    {
        _logger = logger;
        _exporters = new Dictionary<string, IDocumentExporter>(StringComparer.OrdinalIgnoreCase)
        {
            { "excel", excelExporter },
            { "xlsx", excelExporter },
            { "pdf", pdfExporter },
            { "word", wordExporter },
            { "docx", wordExporter },
            { "doc", wordExporter }
        };
    }

    /// <summary>
    /// Gets the appropriate exporter for the specified format
    /// </summary>
    /// <param name="format">The document format (excel, pdf, word)</param>
    /// <returns>The document exporter for the specified format</returns>
    /// <exception cref="ArgumentException">Thrown when the format is not supported</exception>
    public IDocumentExporter GetExporter(string format)
    {
        if (_exporters.TryGetValue(format, out var exporter))
        {
            _logger?.LogInformation("Found exporter for format: {Format}", format);
            return exporter;
        }

        _logger?.LogWarning("No exporter found for format: {Format}", format);
        throw new ArgumentException($"Unsupported document format: {format}. Supported formats: excel, pdf, word");
    }

    /// <summary>
    /// Gets all supported formats
    /// </summary>
    /// <returns>List of supported format names</returns>
    public IEnumerable<string> GetSupportedFormats()
    {
        return new[] { "excel", "pdf", "word" };
    }
}

