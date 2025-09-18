using Andy.Agentic.Domain.Interfaces.Llm.Semantic;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;

public static class DocumentTextExtractorFactory
{
    public static IDocumentTextExtractor GetExtractor(string fileExtension) =>
        fileExtension.ToLowerInvariant() switch
        {
            "txt" => new PlainTextExtractor(),
            "pdf" => new PdfTextExtractor(),
            "docx" => new WordTextExtractor(),
            "xlsx" => new ExcelTextExtractor(),
            "pptx" => new PowerPointTextExtractor(),
            _ => throw new NotSupportedException($"File type '{fileExtension}' is not supported.")
        };
}
