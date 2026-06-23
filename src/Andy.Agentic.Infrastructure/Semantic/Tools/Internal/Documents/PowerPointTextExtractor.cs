using System.Text;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;

public class PowerPointTextExtractor : IDocumentTextExtractor
{
    public Task<string> ExtractTextAsync(byte[] binaryContent)
    {
        try
        {
            using var memoryStream = new MemoryStream(binaryContent);
            using var presentation = PresentationDocument.Open(memoryStream, false);
            var sb = new StringBuilder();

            foreach (var slidePart in presentation.PresentationPart!.SlideParts)
            {
                foreach (var paragraph in slidePart.Slide.Descendants<A.Paragraph>())
                {
                    foreach (var text in paragraph.Descendants<A.Text>())
                    {
                        sb.Append(text.Text);
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
}
