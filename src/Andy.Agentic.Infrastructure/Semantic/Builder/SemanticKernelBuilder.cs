using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Andy.Agentic.Domain.Interfaces.Llm.Semantic;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Models.Semantic;
using Andy.Agentic.Infrastructure.Semantic.Interceptor;
using Andy.Agentic.Infrastructure.Semantic.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Agent = Andy.Agentic.Domain.Models.Agent;

namespace Andy.Agentic.Infrastructure.Semantic.Builder;

/// <summary>
/// Represents a builder for creating instances of SemanticKernel.
/// Implements the ISemanticKernelBuilder interface.
/// </summary>
public class SemanticKernelBuilder : ISemanticKernelBuilder
{
    private readonly IProviderDetector _providerDetector;
    private readonly IAiServiceFactory _aiServiceFactory;
    private readonly IToolManager _toolManager;
    private readonly SkillPluginFactory _skillPluginFactory;
    private readonly ILogger<SemanticKernelBuilder>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticKernelBuilder"/> class.
    /// </summary>
    /// <param name="providerDetector">The provider detector used to identify the AI service provider.</param>
    /// <param name="aiServiceFactory">The factory used to create AI services.</param>
    /// <param name="toolManager">The manager responsible for handling tools.</param>
    /// <param name="logger">Optional logger for logging information.</param>
    public SemanticKernelBuilder(
            IProviderDetector providerDetector,
            IAiServiceFactory aiServiceFactory,
            IToolManager toolManager,
            SkillPluginFactory skillPluginFactory,
            ILogger<SemanticKernelBuilder>? logger = null)
    {
        _providerDetector = providerDetector;
        _aiServiceFactory = aiServiceFactory;
        _toolManager = toolManager;
        _skillPluginFactory = skillPluginFactory;
        _logger = logger;
    }

    /// <summary>
    /// Calls the agent asynchronously and streams chat message content.
    /// Intercepts streaming responses to detect function calls.
    /// </summary>
    /// <param name="query">The kernel response containing chat history and kernel information.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="StreamingChatMessageContent"/> representing the agent's responses.
    /// </returns>
    [Experimental("SKEXP0130")]
    public async IAsyncEnumerable<StreamingChatMessageContent> CallAgentAsync(KernelResponse query)
    {
        await foreach (var item in CallAgentAsync(query, CancellationToken.None))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Calls the agent asynchronously and streams chat message content.
    /// Intercepts streaming responses to detect function calls.
    /// </summary>
    /// <param name="query">The kernel response containing chat history and kernel information.</param>
    /// <param name="cancellationToken">Cancellation token to stop the streaming operation.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="StreamingChatMessageContent"/> representing the agent's responses.
    /// </returns>
    [Experimental("SKEXP0130")]
    public async IAsyncEnumerable<StreamingChatMessageContent> CallAgentAsync(KernelResponse query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        AgentThread thread = new ChatHistoryAgentThread(chatHistory: query.ChatHistory);


        if (query.Agent?.AgentDocuments.Any() == true && query.Agent.EmbeddingLlmConfig != null)
        {
            try
            {

                _logger?.LogInformation("RAG provider added to agent thread for agent {AgentId}", query.Agent.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to add RAG provider for agent {AgentId}", query.Agent.Id);
            }
        }
        var hasSkills = query.Agent?.Skills.Any(s => s.IsActive) ?? false;
        var hastTools = (query.Agent?.Tools.Any() ?? false) || hasSkills;

        OpenAIPromptExecutionSettings? executionSettings = null;
        if (hastTools)
        {
            executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };
        }

        ChatCompletionAgent agent =
            new()
            {
                Name = "agent",
                Kernel = query.Kernel,
                Arguments = executionSettings != null ? new KernelArguments(executionSettings) : null,
                UseImmutableKernel = hastTools
            };

        var prompt = string.Join("\n", query.Agent?.Prompts.Select(p => p.Content) ?? []);

        if (hasSkills && query.Agent is not null)
        {
            var skillCatalog = SkillPluginFactory.BuildCatalogPrompt(query.Agent);
            if (!string.IsNullOrEmpty(skillCatalog))
            {
                prompt = string.IsNullOrWhiteSpace(prompt) ? skillCatalog : $"{prompt}\n\n{skillCatalog}";
            }
        }

        _logger?.LogInformation(
            "CallAgentAsync starting: agentId={AgentId}, sessionHistoryMessages={HistoryCount}, hasTools={HasTools}, promptChars={PromptLength}",
            query.Agent?.Id,
            query.ChatHistory.Count,
            hastTools,
            prompt.Length);

        var chunkIndex = 0;
        var yieldedCount = 0;
        var skippedNullContent = 0;

        await foreach (var response in agent.InvokeStreamingAsync(prompt, thread, cancellationToken: cancellationToken))
        {
            chunkIndex++;
            var content = response.Message.Content;
            if (content is not null)
            {
                yieldedCount++;
                if (yieldedCount == 1)
                {
                    _logger?.LogInformation(
                        "CallAgentAsync first chunk at index {ChunkIndex}, contentLength={ContentLength}",
                        chunkIndex,
                        content.Length);
                }

                yield return response;
            }
            else
            {
                skippedNullContent++;
                _logger?.LogDebug(
                    "CallAgentAsync chunk #{ChunkIndex} skipped (null Content), itemCount={ItemCount}",
                    chunkIndex,
                    response.Message.Items?.Count ?? 0);
            }
        }

        _logger?.LogInformation(
            "CallAgentAsync completed: totalChunks={TotalChunks}, yielded={Yielded}, skippedNullContent={Skipped}",
            chunkIndex,
            yieldedCount,
            skippedNullContent);
    }

    /// <summary>
    /// Builds a Semantic Kernel asynchronously using the provided agent, session, configuration, request, and tool execution recorder.
    /// </summary>
    /// <param name="agent">The agent used for building the kernel.</param>
    /// <param name="session">The session identifier.</param>
    /// <param name="config">The configuration for the LLM.</param>
    /// <param name="request">The request containing details for the kernel build.</param>
    /// <param name="toolExecutionRecorder">The recorder for tool execution.</param>
    /// <returns>
    /// Returns a KernelResponse containing the built kernel and chat history.
    /// </returns>
    public KernelResponse BuildKernelAsync(Agent agent,
        string session,
        LlmConfig config,
        LlmRequest request,
        ToolExecutionRecorder toolExecutionRecorder)
    {
        _logger?.LogInformation(
            "Building Semantic Kernel for agent {AgentId}: model={Model}, provider={Provider}, baseUrl={BaseUrl}, messageCount={MessageCount}, toolCount={ToolCount}, imageCount={ImageCount}",
            agent.Id,
            config.Model,
            _providerDetector.DetectProvider(config),
            config.BaseUrl,
            request.Messages.Count,
            request.Tools?.Count ?? 0,
            request.Images?.Count ?? 0);

        var provider = _providerDetector.DetectProvider(config);
        var builder = Kernel.CreateBuilder();

        _aiServiceFactory.AddAiService(builder, config, provider);

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        var kernel = builder.Build();

        kernel.FunctionInvocationFilters.Add(new FunctionInterceptorFilter(toolExecutionRecorder, agent, session, request.Tools!));


        if (request.Tools?.Any() == true)
        {
            _toolManager.AddToolsAsync(kernel, agent, request.Tools);
        }

        // Import the per-agent skills plugin (progressive disclosure over registry skills).
        _skillPluginFactory.AddSkills(kernel, agent);

        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();

        var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == "user");

        foreach (var message in request.Messages)
        {
            if (message.Role == "user")
            {
                // Check if this message has images (from history or current request)
                var messageImages = message.Images ?? (message == lastUserMessage ? request.Images : null);
                var hasMessageImages = messageImages != null && messageImages.Any();

                if (hasMessageImages)
                {
                    // For Semantic Kernel, create ChatMessageContentItemCollection for multimodal support
                    var contentItems = new Microsoft.SemanticKernel.ChatCompletion.ChatMessageContentItemCollection();

                    // Add text content if present
                    if (!string.IsNullOrEmpty(message.Content))
                    {
                        contentItems.Add(new Microsoft.SemanticKernel.TextContent(message.Content));
                    }

                    // Add image content
                    foreach (var image in messageImages!)
                    {
                        // Extract base64 data (remove data URI prefix if present)
                        var base64Data = image.Data;
                        if (base64Data.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        {
                            var commaIndex = base64Data.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                base64Data = base64Data.Substring(commaIndex + 1);
                            }
                        }

                        try
                        {
                            var imageBytes = Convert.FromBase64String(base64Data);
                            // Create ImageContent with bytes and mime type
                            var imageContent = new Microsoft.SemanticKernel.ImageContent(imageBytes, image.MimeType);
                            contentItems.Add(imageContent);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Failed to decode image data, skipping image: {Error}", ex.Message);
                        }
                    }

                    if (contentItems.Count > 0)
                    {
                        chatHistory.AddUserMessage(contentItems);
                    }
                    else
                    {
                        _logger?.LogWarning("Skipping user message with no usable text or image content");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    chatHistory.AddUserMessage(message.Content);
                }
                else
                {
                    _logger?.LogWarning("Skipping empty user message when building Semantic Kernel chat history");
                }
            }
            else if (!string.IsNullOrWhiteSpace(message.Content))
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
            else
            {
                _logger?.LogWarning("Skipping empty assistant message when building Semantic Kernel chat history");
            }
        }


        _logger?.LogInformation(
            "Semantic Kernel built for agent {AgentId}: chatHistoryMessages={ChatHistoryCount}",
            agent.Id,
            chatHistory.Count);

        return new KernelResponse(kernel, chatHistory, agent);
    }
}
