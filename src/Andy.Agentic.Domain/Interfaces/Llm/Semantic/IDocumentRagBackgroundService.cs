namespace Andy.Agentic.Domain.Interfaces.Llm.Semantic;

/// <summary>
/// Interface for the document RAG background processing service.
/// </summary>
public interface IDocumentRagBackgroundService
{
    /// <summary>
    /// Queues a document for RAG processing.
    /// </summary>
    /// <param name="documentId">The document ID to process.</param>
    /// <param name="agentId">The agent ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task QueueDocumentForProcessingAsync(Guid? documentId, Guid agentId);

    /// <summary>
    /// Gets the current status of the background service.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the number of items currently in the queue.
    /// </summary>
    int QueueCount { get; }
}
