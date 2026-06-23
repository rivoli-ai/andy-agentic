using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Export;

/// <summary>
/// Exports content to Excel format
/// </summary>
public class ExcelExporter : IDocumentExporter
{
    private readonly ILogger<ExcelExporter>? _logger;

    public string FileExtension => ".xlsx";
    public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ExcelExporter(ILogger<ExcelExporter>? logger = null)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(string content, string title)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(title);

                // Parse content - handle both plain text and structured data
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                // Check if content looks like tabular data (contains tabs or pipes)
                var isTabullar = lines.Any(line => line.Contains('\t') || line.Contains('|'));

                if (isTabullar)
                {
                    // Handle tabular data
                    var row = 1;
                    foreach (var line in lines)
                    {
                        var cells = line.Split(new[] { '\t', '|' }, StringSplitOptions.TrimEntries);
                        for (var col = 0; col < cells.Length; col++)
                        {
                            worksheet.Cell(row, col + 1).Value = cells[col];
                        }
                        row++;
                    }
                    
                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();
                    
                    // Format header if exists
                    if (lines.Length > 0)
                    {
                        worksheet.Row(1).Style.Font.Bold = true;
                        worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }
                else
                {
                    // Handle plain text - put each line in a row
                    worksheet.Cell(1, 1).Value = "Content";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    
                    for (var i = 0; i < lines.Length; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = lines[i];
                    }
                    
                    worksheet.Column(1).Width = 100;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting to Excel");
                throw;
            }
        });
    }
}

