using Andy.Agentic.Domain.Interfaces.Llm.Semantic;using Andy.Agentic.Domain.Models;using Andy.Agentic.Domain.Models.Semantic;using Andy.Agentic.Infrastructure.Semantic.Interceptor;using Microsoft.Extensions.DependencyInjection;using Microsoft.Extensions.Logging;using Microsoft.SemanticKernel;using Microsoft.SemanticKernel.Agents;using Microsoft.SemanticKernel.Connectors.OpenAI;using Agent = Andy.Agentic.Domain.Models.Agent;namespace Andy.Agentic.Infrastructure.Semantic.Builder;/// <summary>
/// Represents a builder for creating instances of SemanticKernel.
/// Implements the ISemanticKernelBuilder interface.
/// </summary>
public class SemanticKernelBuilder : ISemanticKernelBuilder{    private readonly IProviderDetector _providerDetector;    private readonly IAiServiceFactory _aiServiceFactory;    private readonly IToolManager _toolManager;    private readonly ILogger<SemanticKernelBuilder>? _logger;

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
            ILogger<SemanticKernelBuilder>? logger = null)    {        _providerDetector = providerDetector;        _aiServiceFactory = aiServiceFactory;        _toolManager = toolManager;        _logger = logger;    }

    /// <summary>
    /// Calls the agent asynchronously and streams chat message content.
    /// Intercepts streaming responses to detect function calls.
    /// </summary>
    /// <param name="query">The kernel response containing chat history and kernel information.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="StreamingChatMessageContent"/> representing the agent's responses.
    /// </returns>
    public async IAsyncEnumerable<StreamingChatMessageContent> CallAgentAsync(KernelResponse query)    {        AgentThread thread = new ChatHistoryAgentThread(chatHistory: query.ChatHistory);        ChatCompletionAgent agent =            new()            {                Name = "agent",                Kernel = query.Kernel,                Arguments = new KernelArguments(                    new OpenAIPromptExecutionSettings()                    {                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),                    }),

            };        await foreach (var response in agent.InvokeStreamingAsync(thread))        {
            // Intercept streaming responses to detect function calls
            if (response.Message.Content != null)            {
                // Check if this is a function call response
                if (IsFunctionCallResponse(response.Message))                {                    Console.WriteLine($"Function call detected in stream: {response.Message.Content}");                }                yield return response;            }        }    }    /// <summary>    /// Determines if the given response contains indicators of a function call.    /// </summary>    /// <param name="response">The response to check.</param>    /// <returns>True if the response contains function call indicators; otherwise, false.</returns>    private bool IsFunctionCallResponse(StreamingChatMessageContent response)    {        return response.Content?.Contains("function") == true ||               response.Content?.Contains("tool_call") == true;    } 


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
    public KernelResponse BuildKernelAsync(
            Agent agent,
            string session,
            LlmConfig config,
            LlmRequest request,
            ToolExecutionRecorder toolExecutionRecorder)    {        _logger?.LogInformation("Building Semantic Kernel...");        var provider = _providerDetector.DetectProvider(config);        var builder = Kernel.CreateBuilder();        _aiServiceFactory.AddAiService(builder, config, provider);        builder.Services.AddLogging(logging =>        {            logging.AddConsole();            logging.SetMinimumLevel(LogLevel.Debug);
        });        var kernel = builder.Build();        kernel.FunctionInvocationFilters.Add(new FunctionInterceptorFilter(toolExecutionRecorder, agent, session, request.Tools!));        if (request.Tools?.Any() == true)        {            _toolManager.AddToolsAsync(kernel, request.Tools);        }        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();        foreach (var message in request.Messages)        {            if (message.Role == "user")            {                chatHistory.AddUserMessage(message.Content);            }            else            {                chatHistory.AddAssistantMessage(message.Content);            }        }        return new KernelResponse(kernel, chatHistory);    }}