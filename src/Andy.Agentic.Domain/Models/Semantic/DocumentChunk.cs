using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Andy.Agentic.Domain.Models.Semantic;

public sealed class DataModel
{
    [VectorStoreKey]
    [TextSearchResultName]
    public required string Key { get; init; }
    [VectorStoreData]
    [TextSearchResultValue]
    public required string Text { get; init; }

    [VectorStoreData]
    public required string SourceName { get; init; }
    [VectorStoreData]
    public required string SourceLink { get; init; }

    [VectorStoreVector(4096)]
    public string Embedding => this.Text;
}
