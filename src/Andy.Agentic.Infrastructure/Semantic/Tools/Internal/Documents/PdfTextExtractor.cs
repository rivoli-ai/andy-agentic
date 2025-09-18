using System.Text;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using UglyToad.PdfPig;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;

public class PdfTextExtractor : IDocumentTextExtractor
{
    public async Task<string> ExtractTextAsync(byte[] binaryContent)
    {
        try
        {
            await using var memoryStream = new MemoryStream(binaryContent);
            using var pdf = PdfDocument.Open(memoryStream);
            var textBuilder = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                    textBuilder.AppendLine(page.Text);
            }

            return textBuilder.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

   
}
