using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;

/// <summary>
/// Exports content to Word format
/// </summary>
public class WordExporter : IDocumentExporter
{
    private readonly ILogger<WordExporter>? _logger;

    public string FileExtension => ".docx";
    public string MimeType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public WordExporter(ILogger<WordExporter>? logger = null)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(string content, string title)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    // Add main document part
                    var mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = mainPart.Document.AppendChild(new Body());

                    // Add title
                    var titleParagraph = body.AppendChild(new Paragraph());
                    var titleRun = titleParagraph.AppendChild(new Run());
                    var titleRunProperties = titleRun.AppendChild(new RunProperties());
                    titleRunProperties.AppendChild(new Bold());
                    titleRunProperties.AppendChild(new FontSize { Val = "32" }); // 16pt
                    titleRun.AppendChild(new Text(title));

                    // Add empty line
                    body.AppendChild(new Paragraph());

                    // Add content
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var paragraph = body.AppendChild(new Paragraph());
                        var run = paragraph.AppendChild(new Run());
                        
                        // Check if line looks like a header
                        if (line.Length < 50 && line.ToUpper() == line && !line.Contains(' '))
                        {
                            var runProperties = run.AppendChild(new RunProperties());
                            runProperties.AppendChild(new Bold());
                            runProperties.AppendChild(new FontSize { Val = "24" }); // 12pt
                        }
                        
                        run.AppendChild(new Text(line));
                    }

                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting to Word");
                throw;
            }
        });
    }
}

