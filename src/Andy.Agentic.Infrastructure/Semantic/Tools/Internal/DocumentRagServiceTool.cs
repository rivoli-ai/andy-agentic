using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;

namespace Andy.Agentic.Infrastructure.Semantic.Tools.Internal;

/// <summary>
/// Tool that provides document search functionality using DocumentRagService.
/// </summary>
public class DocumentRagServiceTool(
    IDocumentRagProvider documentProvider,
    ILogger<DocumentRagServiceTool>? logger = null)
{
    /// <summary>
    /// Searches through documents using RAG functionality.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="agent"></param>
    /// <returns>Search results from the document RAG system.</returns>
    public async Task<string> SearchDocumentsAsync(
        [Description("The search query to find relevant documents")] string query, Agent agent)
    {
        try
        {
            logger?.LogInformation("Searching documents for query: {Query} in agent: {AgentId}", query, agent.Id);


            return await documentProvider.GetSearchDocumentsAsync(query, agent);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error searching documents for query: {Query} in agent: {AgentId}", query, agent.Id);
            return $"Error occurred while searching documents: {ex.Message}";
        }
    }
}

