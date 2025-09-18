namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(byte[] binaryContent);
}
