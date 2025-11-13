using Andy.Agentic.Domain.Models;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Andy.Agentic.Infrastructure.Tests.Tools;

/// <summary>
/// Tests for DocumentExportTool
/// </summary>
public class DocumentExportToolTests
{
    private readonly DocumentExportTool _tool;
    private readonly Mock<ILogger<DocumentExportTool>> _mockLogger;
    private readonly Mock<ILogger<DocumentExportFactory>> _mockFactoryLogger;
    private readonly Mock<ILogger<ExcelExporter>> _mockExcelLogger;
    private readonly Mock<ILogger<PdfExporter>> _mockPdfLogger;
    private readonly Mock<ILogger<WordExporter>> _mockWordLogger;
    private readonly Tool _toolConfig;

    /// <summary>
    /// Constructor that sets up the test environment
    /// </summary>
    public DocumentExportToolTests()
    {
        _mockLogger = new Mock<ILogger<DocumentExportTool>>();
        _mockFactoryLogger = new Mock<ILogger<DocumentExportFactory>>();
        _mockExcelLogger = new Mock<ILogger<ExcelExporter>>();
        _mockPdfLogger = new Mock<ILogger<PdfExporter>>();
        _mockWordLogger = new Mock<ILogger<WordExporter>>();

        // Setup tool configuration with apiUrl
        _toolConfig = new Tool
        {
            Id = Guid.NewGuid(),
            Name = "Export",
            Description = "Export documents",
            Type = "InternalTool",
            Configuration = "{\"apiUrl\":\"https://localhost\"}"
        };

        var excelExporter = new ExcelExporter(_mockExcelLogger.Object);
        var pdfExporter = new PdfExporter(_mockPdfLogger.Object);
        var wordExporter = new WordExporter(_mockWordLogger.Object);

        var factory = new DocumentExportFactory(
            excelExporter,
            pdfExporter,
            wordExporter,
            _mockFactoryLogger.Object
        );

        _tool = new DocumentExportTool(factory, _mockLogger.Object);
    }

    /// <summary>
    /// Test that exporting to Excel format works
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_ToExcel_ShouldReturnBase64EncodedExcel()
    {
        // Arrange
        var content = "Test content for Excel export";
        var format = "excel";
        var title = "Test Document";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, title, _toolConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("**Document Export Complete**", result);
        Assert.Contains("**Title:** Test Document", result);
        Assert.Contains("**Format:** EXCEL", result);
        Assert.Contains("[Click here to download", result);
        Assert.Contains("https://localhost/api/exports/", result);
        Assert.Contains("has been exported successfully", result);
    }

    /// <summary>
    /// Test that exporting to PDF format works
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_ToPdf_ShouldReturnBase64EncodedPdf()
    {
        // Arrange
        var content = "Test content for PDF export\nThis is a multi-line document.";
        var format = "pdf";
        var title = "Test PDF";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, title, _toolConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("**Document Export Complete**", result);
        Assert.Contains("**Title:** Test PDF", result);
        Assert.Contains("**Format:** PDF", result);
        Assert.Contains("[Click here to download", result);
        Assert.Contains("https://localhost/api/exports/", result);
        Assert.Contains("has been exported successfully", result);
    }

    /// <summary>
    /// Test that exporting to Word format works
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_ToWord_ShouldReturnBase64EncodedWord()
    {
        // Arrange
        var content = "Test content for Word export\nWith multiple lines\nAnd more content.";
        var format = "word";
        var title = "Test Word Document";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, title, _toolConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("**Document Export Complete**", result);
        Assert.Contains("**Title:** Test Word Document", result);
        Assert.Contains("**Format:** WORD", result);
        Assert.Contains("[Click here to download", result);
        Assert.Contains("https://localhost/api/exports/", result);
        Assert.Contains("has been exported successfully", result);
    }

    /// <summary>
    /// Test that exporting with unsupported format returns an error
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_WithUnsupportedFormat_ShouldReturnError()
    {
        // Arrange
        var content = "Test content";
        var format = "unsupported";
        var title = "Test";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, title, _toolConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error occurred while exporting document", result);
        Assert.Contains("Supported formats are", result);
    }

    /// <summary>
    /// Test that listing supported formats works
    /// </summary>
    [Fact]
    public async Task ListSupportedFormatsAsync_ShouldReturnAllFormats()
    {
        // Act
        var result = await _tool.ListSupportedFormatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Supported Document Export Formats", result);
        Assert.Contains("EXCEL", result);
        Assert.Contains("PDF", result);
        Assert.Contains("WORD", result);
    }

    /// <summary>
    /// Test that exporting with null title uses default title
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_WithNullTitle_ShouldUseDefaultTitle()
    {
        // Arrange
        var content = "Test content";
        var format = "excel";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, null, _toolConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("**Title:** Exported Document", result);
    }

    /// <summary>
    /// Test Excel exporter with tabular data
    /// </summary>
    [Fact]
    public async Task ExcelExporter_WithTabularData_ShouldCreateTableStructure()
    {
        // Arrange
        var exporter = new ExcelExporter(_mockExcelLogger.Object);
        var content = "Name\tAge\tCity\nJohn\t30\tNew York\nJane\t25\tLos Angeles";
        var title = "Test Table";

        // Act
        var result = await exporter.ExportAsync(content, title);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// Test to display the exact output format
    /// </summary>
    [Fact]
    public async Task ExportDocumentAsync_OutputFormat_ShouldMatchExpectedStructure()
    {
        // Arrange
        var content = "Sample content";
        var format = "pdf";
        var title = "Sample Document";

        // Act
        var result = await _tool.ExportDocumentAsync(content, format, title, _toolConfig);

        // Assert
        Assert.NotNull(result);
        
        // Output for debugging
        Console.WriteLine("=== EXPORT TOOL OUTPUT ===");
        Console.WriteLine(result);
        Console.WriteLine("=== END OUTPUT ===");

        // Verify structure
        Assert.Contains("📄 **Document Export Complete**", result);
        Assert.Contains("**Title:**", result);
        Assert.Contains("**Format:**", result);
        Assert.Contains("**Size:**", result);
        Assert.Contains("**📥 Download:**", result);
        Assert.Contains("[Click here to download", result);
        Assert.Contains("](https://localhost/api/exports/", result);
        Assert.Contains(".pdf)", result);
    }
}

