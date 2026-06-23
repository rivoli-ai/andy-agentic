using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using DocumentFormat.OpenXml.Packaging;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;

public class WordTextExtractor : IDocumentTextExtractor
{
    public Task<string> ExtractTextAsync(byte[] binaryContent)
    {
        try
        {
            using var memoryStream = new MemoryStream(binaryContent);
            using var wordDoc = WordprocessingDocument.Open(memoryStream, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;
            return Task.FromResult(body?.InnerText ?? string.Empty);
        }
        catch
        {
            return Task.FromResult(string.Empty);
        }
    }
}
