using System.ClientModel;
using System.Text;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Andy.Agentic.Infrastructure.Semantic.Tools.Internal.Documents;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Npgsql;
using OpenAI;
using UglyToad.PdfPig;

namespace Andy.Agentic.Infrastructure.Semantic.Provider;

/// <summary>
/// Provides RAG (Retrieval-Augmented Generation) functionality for documents using PostgresSQL with PgVector.
/// Handles document processing, chunking, embedding generation, and vector storage/retrieval.
/// </summary>
public class DocumentRagProvider(
    IDocumentRepository documentRepository,
    IAgentDocumentRepository agentDocumentRepository,
    NpgsqlDataSource dataSource)
    : IDocumentRagProvider
{
    private const int MaxChunkSize = 1000;
    private const int OverlapSize = 200;
    private const int EmbeddingDimensions = 4096;
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

    /// <summary>
    /// Searches for relevant document chunks based on the provided query using semantic similarity.
    /// </summary>
    /// <param name="query">The search query to find relevant documents</param>
    /// <param name="agent">The agent context for which to search documents</param>
    /// <returns>Formatted string containing search results with relevance scores</returns>
    public async Task<string> GetSearchDocumentsAsync(string query, Agent agent)
    {
        var collection = await GetCollectionAsync(agent);

        var searchResults = collection.SearchAsync(query, top: 10);
        var formattedResults = new StringBuilder();
        formattedResults.AppendLine("## Search Results");
        formattedResults.AppendLine();

        await foreach (var result in searchResults)
        {
            formattedResults.AppendLine($"**Source:** {result.Record.SourceName}");
            formattedResults.AppendLine($"**Score:** {result.Score:F4}");
            formattedResults.AppendLine($"**Content:** {result.Record.Text}");
            formattedResults.AppendLine($"**Link:** {result.Record.SourceLink}");
            formattedResults.AppendLine("---");
            formattedResults.AppendLine();
        }

        return formattedResults.ToString();
    }

    /// <summary>
    /// Removes all document chunks associated with a specific document from the RAG collection.
    /// </summary>
    /// <param name="documentId">The ID of the document to remove from RAG</param>
    /// <param name="agent">The agent context from which to remove the document</param>
    public async Task RemoveDocumentFromRagAsync(Guid documentId, Agent agent)
    {
        var document = await documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            return;
        }

        var collection = await GetCollectionAsync(agent);

        var searchResults = collection.SearchAsync($"document:{documentId}", top: 1000);
        var keysToDelete = new List<string>();

        await foreach (var result in searchResults)
        {
            // Check if this chunk belongs to the document we want to remove
            if (result.Record.SourceLink.Contains(documentId.ToString()))
            {
                keysToDelete.Add(result.Record.Key);
            }
        }

        // Delete all found chunks
        if (keysToDelete.Any())
        {
            await collection.DeleteAsync(keysToDelete);
        }
    }

    /// <summary>
    /// Processes all documents associated with an agent for RAG functionality.
    /// This includes extracting text content, chunking, and storing embeddings.
    /// </summary>
    /// <param name="agent">The agent for which to process documents</param>
    public async Task ProcessAllDocumentsForAgentAsync(Agent agent)
    {
        var agentDocuments = await agentDocumentRepository.GetByAgentIdAsync(agent.Id);

        var processingTasks = agentDocuments.Select(agentDocument =>
            ProcessDocumentForRagAsync(agentDocument.Document.Id, agent));

        await Task.WhenAll(processingTasks);
    }

    /// <summary>
    /// Processes a single document for RAG by extracting text, chunking it, and storing embeddings.
    /// </summary>
    /// <param name="documentId">The ID of the document to process</param>
    /// <param name="agent">The agent context for processing</param>
    public async Task ProcessDocumentForRagAsync(Guid? documentId, Agent agent)
    {
        if (!documentId.HasValue)
        {
            return;
        }

        var document = await documentRepository.GetByIdAsync(documentId.Value);
        if (document == null)
        {
            return;
        }

        var textContent = await ExtractTextContentAsync(document);
        if (string.IsNullOrWhiteSpace(textContent))
        {
            return;
        }

        var chunks = ChunkText(textContent);
        var dataModels = CreateDataModels(chunks, document, documentId.Value);
        var collection = await GetCollectionAsync(agent);

        await collection.UpsertAsync(dataModels);
    }

    /// <summary>
    /// Creates a PostgresSQL collection configured for vector similarity search with embeddings.
    /// </summary>
    /// <param name="agent">The agent for which to create/get the collection</param>
    /// <returns>Configured PostgresSQL collection for vector operations</returns>
    private async Task<PostgresCollection<string, DataModel>> GetCollectionAsync(Agent agent)
    {
        var collectionOptions = new PostgresCollectionOptions
        {
            EmbeddingGenerator = CreateEmbeddingGenerator(agent.EmbeddingLlmConfig!)
        };

        //var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        //dataSourceBuilder.UseVector();
        //var dataSource = dataSourceBuilder.Build();

        var collection = new PostgresCollection<string, DataModel>(
            dataSource: dataSource,
            name: $"Agent_{agent.Id}",
            ownsDataSource: true,
            options: collectionOptions);

        await collection.EnsureCollectionExistsAsync();
        return collection;
    }

    /// <summary>
    /// Extracts text content from a document entity, supporting both text and PDF formats.
    /// </summary>
    /// <param name="document">The document entity to extract text from</param>
    /// <returns>Extracted text content or empty string if extraction fails</returns>
    private static async Task<string> ExtractTextContentAsync(Domain.Entities.DocumentEntity document)
    {
        // Return existing text content if available
        if (!string.IsNullOrWhiteSpace(document.Content))
        {
            return document.Content;
        }

        // Extract from binary content (PDF)
        if (document.BinaryContent is not { Length: > 0 })
        {
            return string.Empty;
        }

        var extractor = DocumentTextExtractorFactory.GetExtractor(document.Type);

        return await extractor.ExtractTextAsync(document.BinaryContent);
    }


    /// <summary>
    /// Splits text into chunks with overlap for better context preservation in embeddings.
    /// </summary>
    /// <param name="text">Text to be chunked</param>
    /// <returns>List of text chunks with controlled size and overlap</returns>
    private static List<string> ChunkText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var chunks = new List<string>();
        var sentences = SplitIntoSentences(text);
        var currentChunk = new StringBuilder();

        foreach (var sentence in sentences)
        {
            if (ShouldCreateNewChunk(currentChunk, sentence))
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk = CreateOverlappingChunk(chunks);
                }
            }

            AppendSentenceToChunk(currentChunk, sentence);
        }

        // Add final chunk if it has content
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }

        return chunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk)).ToList();
    }

    /// <summary>
    /// Splits text into sentences using common sentence terminators.
    /// </summary>
    private static string[] SplitIntoSentences(string text) =>
        text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

    /// <summary>
    /// Determines if a new chunk should be created based on current chunk size and new sentence.
    /// </summary>
    private static bool ShouldCreateNewChunk(StringBuilder currentChunk, string sentence) =>
        currentChunk.Length > 0 &&
        currentChunk.Length + sentence.Length + 2 > MaxChunkSize; // +2 for ". "

    /// <summary>
    /// Creates a new chunk with overlap from the previous chunk to maintain context.
    /// </summary>
    private static StringBuilder CreateOverlappingChunk(List<string> chunks)
    {
        var newChunk = new StringBuilder();
        var lastChunk = chunks.LastOrDefault();

        if (!string.IsNullOrEmpty(lastChunk) && lastChunk.Length > OverlapSize)
        {
            var overlap = lastChunk[^OverlapSize..];
            newChunk.Append(overlap).Append(" ");
        }

        return newChunk;
    }

    /// <summary>
    /// Appends a sentence to the current chunk with proper formatting.
    /// </summary>
    private static void AppendSentenceToChunk(StringBuilder currentChunk, string sentence) => currentChunk.Append(sentence).Append(". ");

    /// <summary>
    /// Creates DataModel objects from text chunks for storage in the vector database.
    /// </summary>
    /// <param name="chunks">Text chunks to convert</param>
    /// <param name="document">Source document entity</param>
    /// <param name="documentId">Document identifier</param>
    /// <returns>Array of DataModel objects ready for storage</returns>
    private static DataModel[] CreateDataModels(
        List<string> chunks,
        Domain.Entities.DocumentEntity document,
        Guid documentId) =>
        chunks.Select((chunk, index) => new DataModel
        {
            Text = chunk,
            Key = $"{documentId}_{index}",
            SourceName = document.Name,
            SourceLink = $"/documents/{documentId}",
        }).ToArray();

    /// <summary>
    /// Creates an embedding generator client configured with the provided LLM configuration.
    /// </summary>
    /// <param name="config">LLM configuration containing API credentials and model details</param>
    /// <returns>Configured embedding generator for creating text embeddings</returns>
    private static IEmbeddingGenerator CreateEmbeddingGenerator(LlmConfig config)
    {
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(config.BaseUrl)
        };

        var openAiClient = new OpenAIClient(new ApiKeyCredential(config.ApiKey), clientOptions);
        return openAiClient
            .GetEmbeddingClient(config.Model)
            .AsIEmbeddingGenerator(EmbeddingDimensions);
    }
}
