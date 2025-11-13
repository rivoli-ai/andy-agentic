using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;

/// <summary>
/// Exports content to PDF format
/// </summary>
public class PdfExporter : IDocumentExporter
{
    private readonly ILogger<PdfExporter>? _logger;

    public string FileExtension => ".pdf";
    public string MimeType => "application/pdf";

    public PdfExporter(ILogger<PdfExporter>? logger = null)
    {
        _logger = logger;
        // Configure QuestPDF for community usage
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportAsync(string content, string title)
    {
        return await Task.Run(() =>
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        // Header
                        page.Header()
                            .Text(title)
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        // Content
                        page.Content().Markdown(content);

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Page ").FontSize(9);
                                text.CurrentPageNumber().FontSize(9);
                                text.Span(" of ").FontSize(9);
                                text.TotalPages().FontSize(9);
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting to PDF");
                throw;
            }
        });
    }
}

