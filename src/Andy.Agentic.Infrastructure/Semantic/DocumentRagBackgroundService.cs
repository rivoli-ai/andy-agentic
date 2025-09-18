
using Andy.Agentic.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace Andy.Agentic.Infrastructure.Semantic;

/// <summary>
/// Background service for processing documents for RAG functionality.
/// </summary>
public class DocumentRagBackgroundService : BackgroundService, IDocumentRagBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentRagBackgroundService> _logger;
    private readonly IMapper _mapper;
    private readonly Channel<DocumentRagProcessingRequest> _processingQueue;

    public DocumentRagBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DocumentRagBackgroundService> logger,
        IMapper mapper)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapper = mapper;
        _processingQueue = Channel.CreateUnbounded<DocumentRagProcessingRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        
        _logger.LogInformation("DocumentRagBackgroundService constructor called");
    }

    /// <summary>
    /// Queues a document for RAG processing.
    /// </summary>
    /// <param name="documentId">The document ID to process.</param>
    /// <param name="agentId">The agent ID.</param>
    public async Task QueueDocumentForProcessingAsync(Guid? documentId, Guid agentId)
    {
        _logger.LogInformation("Attempting to queue document {DocumentId} for RAG processing with agent {AgentId}", 
            documentId, agentId);

        var request = new DocumentRagProcessingRequest
        {
            DocumentId = documentId,
            AgentId = agentId,
            RequestedAt = DateTime.UtcNow
        };

        try
        {
            await _processingQueue.Writer.WriteAsync(request);
            _logger.LogInformation("Successfully queued document {DocumentId} for RAG processing with agent {AgentId}", 
                documentId, agentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue document {DocumentId} for RAG processing with agent {AgentId}", 
                documentId, agentId);
            throw;
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DocumentRagBackgroundService StartAsync called");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document RAG Background Service started");

        try
        {
            await foreach (var request in _processingQueue.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation("Processing document {DocumentId} for agent {AgentId}", 
                    request.DocumentId, request.AgentId);
                
                try
                {
                    await ProcessDocumentAsync(request, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document {DocumentId} for agent {AgentId}", 
                        request.DocumentId, request.AgentId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Document RAG Background Service is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in Document RAG Background Service");
        }
    }

    private async Task ProcessDocumentAsync(DocumentRagProcessingRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var documentRagService = scope.ServiceProvider.GetRequiredService<IDocumentRagProvider>();
        var agentService = scope.ServiceProvider.GetRequiredService<IAgentRepository>();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DocumentRagHub>>();

        try
        {
            // Get the agent to check if it has embedding configuration
            var agent = await agentService.GetByIdAsync(request.AgentId);
            if (agent == null)
            {
                _logger.LogWarning("Agent {AgentId} not found for document processing", request.AgentId);
                return;
            }

            if (agent.EmbeddingLlmConfig == null)
            {
                _logger.LogInformation("Skipping RAG processing for document {DocumentId} - agent {AgentId} has no embedding configuration", 
                    request.DocumentId, request.AgentId);
                return;
            }

            // Process the document for RAG
            await documentRagService.ProcessDocumentForRagAsync(request.DocumentId, _mapper.Map<Agent>(agent));
            
            // Update the document's RAG processing status
            if (request.DocumentId.HasValue)
            {
                var document = await documentRepository.GetByIdAsync(request.DocumentId.Value);
                if (document != null)
                {
                    document.IsRagProcessed = true;
                    document.UpdatedAt = DateTime.UtcNow;
                    await documentRepository.UpdateAsync(document);
                    _logger.LogInformation("Updated RAG processing status for document {DocumentId}", request.DocumentId);
                    
                    // Notify frontend about the RAG status update
                    await hubContext.Clients.Group($"Agent_{request.AgentId}").SendAsync("DocumentRagStatusUpdated", new
                    {
                        DocumentId = request.DocumentId.Value,
                        AgentId = request.AgentId,
                        IsRagProcessed = true,
                        UpdatedAt = document.UpdatedAt
                    });
                }
            }
            
            _logger.LogInformation("Successfully processed document {DocumentId} for RAG with agent {AgentId}", 
                request.DocumentId, request.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document {DocumentId} for RAG with agent {AgentId}", 
                request.DocumentId, request.AgentId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document RAG Background Service is stopping");
        _processingQueue.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current status of the background service.
    /// </summary>
    public bool IsRunning => !_processingQueue.Reader.Completion.IsCompleted;

    /// <summary>
    /// Gets the number of items currently in the queue.
    /// </summary>
    public int QueueCount => _processingQueue.Reader.CanCount ? _processingQueue.Reader.Count : -1;
}

/// <summary>
/// Request model for document RAG processing.
/// </summary>
public class DocumentRagProcessingRequest
{
    public Guid? DocumentId { get; set; }
    public Guid AgentId { get; set; }
    public DateTime RequestedAt { get; set; }
}
