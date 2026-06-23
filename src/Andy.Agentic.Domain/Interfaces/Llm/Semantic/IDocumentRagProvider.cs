using System.Diagnostics.CodeAnalysis;
using Andy.Agentic.Domain.Models;
using Microsoft.SemanticKernel.Data;

namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;

/// <summary>
///     Service for managing document-based RAG (Retrieval Augmented Generation) functionality.
/// </summary>
public interface IDocumentRagProvider
{

    Task ProcessDocumentForRagAsync(Guid? documentId, Agent agent);

    Task<string> GetSearchDocumentsAsync(string query, Agent agent);

    /// <summary>
    ///     Removes a document from RAG processing.
    /// </summary>
    /// <param name="documentId">The document ID to remove.</param>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveDocumentFromRagAsync(Guid documentId, Agent agent);

    /// <summary>
    ///     Processes all documents for an agent.
    /// </summary>
    /// <param name="agent">The agent.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessAllDocumentsForAgentAsync(Agent agent);

}
