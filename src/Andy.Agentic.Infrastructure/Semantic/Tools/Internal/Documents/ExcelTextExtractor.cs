using System.Text;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;

public class ExcelTextExtractor : IDocumentTextExtractor
{
    public Task<string> ExtractTextAsync(byte[] binaryContent)
    {
        try
        {
            using var memoryStream = new MemoryStream(binaryContent);
            using var spreadsheet = SpreadsheetDocument.Open(memoryStream, false);
            var sb = new StringBuilder();

            foreach (var sheet in spreadsheet.WorkbookPart.Workbook.Sheets.OfType<Sheet>())
            {
                var wsPart = (WorksheetPart)spreadsheet.WorkbookPart.GetPartById(sheet.Id!);
                var rows = wsPart.Worksheet.GetFirstChild<SheetData>()?.Elements<Row>();
                if (rows == null)
                    continue;

                foreach (var row in rows)
                {
                    foreach (var cell in row.Elements<Cell>())
                    {
                        sb.Append(GetCellText(cell, spreadsheet));
                        sb.Append("\t");
                    }
                    sb.AppendLine();
                }
            }

            return Task.FromResult(sb.ToString());
        }
        catch
        {
            return Task.FromResult(string.Empty);
        }
    }

    private static string GetCellText(Cell cell, SpreadsheetDocument doc)
    {
        if (cell.CellValue == null)
            return string.Empty;

        var value = cell.CellValue.Text;
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sst = doc.WorkbookPart.SharedStringTablePart?.SharedStringTable;
            if (sst != null && int.TryParse(value, out int id))
                return sst.ElementAt(id).InnerText;
        }

        return value;
    }
}
