using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Andy.Agentic.Application.Services;

/// <summary>
/// Service responsible for managing chat operations, including message streaming, session management, and analytics.
/// </summary>
/// <param name="databaseResourceAccess">Database service for data persistence operations.</param>
/// <param name="llmResourceAccess">LLM service for language model interactions.</param>
/// <param name="toolExecutionService">Service for executing tools during chat interactions.</param>
public class ChatService(
    IDataBaseService databaseResourceAccess,
    ILlmService llmResourceAccess,
    IToolExecutionService toolExecutionService,
    ILogger<ChatService> logger)
    : IChatService
{
    private const string UserRole = "user";
    private const string AssistantRole = "assistant";
    private const string AgentModelName = "agent-model";
    private const string ChatCompletionObject = "chat.completion.chunk";
    private const int DefaultTokenCountMultiplier = 1; 


    /// <summary>
    /// Sends a message to an agent and streams the response back asynchronously.
    /// </summary>
    /// <param name="chatMessage">The message to send, including agent ID and content.</param>
    /// <param name="cancellationToken">Cancellation token to stop the streaming operation.</param>
    /// <returns>An async enumerable of response chunks as they are generated.</returns>
    public async IAsyncEnumerable<StreamingResult> SendMessageStreamAsync(ChatMessage chatMessage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateChatMessage(chatMessage);
        if (!validationResult.IsValid)
        {

            yield return new StreamingResult { Content = validationResult.ErrorMessage };
            yield break;
        }

        var agent = await databaseResourceAccess.GetAgentByIdAsync(chatMessage.AgentId!.Value);
        if (agent == null)
        {
            yield return new StreamingResult { Content = "Error: Agent not found" };
            yield break;
        }

        var activePrompt = GetActivePrompt(agent);
        if (activePrompt == null)
        {
            yield return new StreamingResult { Content = "Error: Agent has no active prompt" };
            yield break;
        }

        var sessionId = chatMessage.SessionId ??  await databaseResourceAccess.CreateNewChatSessionAsync(chatMessage.AgentId.Value);

        // Debug: Log images count
        logger.LogInformation(
            "SendMessageStream starting: agentId={AgentId}, sessionId={SessionId}, contentLength={ContentLength}, imageCount={ImageCount}",
            chatMessage.AgentId,
            chatMessage.SessionId,
            chatMessage.Content?.Length ?? 0,
            chatMessage.Images?.Count ?? 0);

        await SaveUserMessageAsync(chatMessage, sessionId);

        await foreach (var chunk in SendMessageStreamRecursiveAsync(agent, activePrompt, chatMessage.Content, sessionId, null, chatMessage.Images, cancellationToken))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// Recursively processes a message stream, handling tool calls and follow-up responses.
    /// </summary>
    /// <param name="agent">The agent processing the message.</param>
    /// <param name="activePrompt">The active prompt for the agent.</param>
    /// <param name="messageContent">The content of the message to process.</param>
    /// <param name="sessionId">The session ID for the conversation.</param>
    /// <param name="toolResults"></param>
    /// <param name="images">Optional list of images attached to the message.</param>
    /// <param name="cancellationToken">Cancellation token to stop the streaming operation.</param>
    /// <returns>An async enumerable of response chunks as they are generated.</returns>
    public async IAsyncEnumerable<StreamingResult> SendMessageStreamRecursiveAsync(Agent agent,
        Prompt activePrompt,
        string messageContent,
        string sessionId,
        List<ToolExecutionLog>? toolResults = null,
        List<ChatImage>? images = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        var recentMessages = await databaseResourceAccess.GetChatHistoryBySessionAsync(sessionId);

        logger.LogInformation(
            "SendMessageStreamRecursiveAsync: sessionId={SessionId}, agentId={AgentId}, historyCount={HistoryCount}, imageCount={ImageCount}",
            sessionId,
            agent.Id,
            recentMessages.Count(),
            images?.Count ?? 0);

        var chatRequest = await llmResourceAccess.PrepareLlmMessageAsync(agent, activePrompt, messageContent, sessionId, recentMessages.ToList(), images);

        var responseContent = new List<string?>();
        var thinkingContent = new List<string?>();
        var recorder = new ToolExecutionRecorder();
        var outboundChunkCount = 0;

        await foreach (var chunk in llmResourceAccess.SendToLlmProviderStreamAsync(agent, chatRequest, sessionId, recorder, cancellationToken))
        {
            outboundChunkCount++;
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "SendMessageStreamRecursiveAsync cancelled: sessionId={SessionId}, chunksSent={ChunkCount}",
                    sessionId,
                    outboundChunkCount);
                await SaveAssistantMessageAsync(agent, sessionId, responseContent, thinkingContent, recorder.Records.ToList());
                cancellationToken.ThrowIfCancellationRequested();
            }
            
            if (!string.IsNullOrEmpty(chunk.Thinking))
            {
                thinkingContent.Add(chunk.Thinking);
            }

            if (!string.IsNullOrEmpty(chunk.Content))
            {
                responseContent.Add(chunk.Content);
            }

            if (!string.IsNullOrEmpty(chunk.Thinking) || !string.IsNullOrEmpty(chunk.Content))
            {
                yield return chunk;
            }
        }

        logger.LogInformation(
            "SendMessageStreamRecursiveAsync completed: sessionId={SessionId}, outboundChunks={ChunkCount}, responseChars={ResponseChars}, thinkingChars={ThinkingChars}",
            sessionId,
            outboundChunkCount,
            string.Concat(responseContent.Where(c => c != null)).Length,
            string.Concat(thinkingContent.Where(c => c != null)).Length);

        // Always save the message
        await SaveAssistantMessageAsync(agent, sessionId, responseContent, thinkingContent, recorder.Records.ToList());

    }



    /// <summary>
    /// Gets a message stream formatted for API consumption.
    /// </summary>
    /// <param name="chatMessage">The message to process and stream.</param>
    /// <param name="cancellationToken">Cancellation token to stop the streaming operation.</param>
    /// <returns>An async enumerable of formatted response objects.</returns>
    public async IAsyncEnumerable<object> GetMessageStreamAsync(ChatMessage chatMessage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sseChunkCount = 0;
        await foreach (var chunk in SendMessageStreamAsync(chatMessage, cancellationToken))
        {
            if (chunk == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(chunk.Thinking))
            {
                sseChunkCount++;
                yield return CreateSseChunk(thinking: chunk.Thinking);
            }

            if (!string.IsNullOrEmpty(chunk.Content))
            {
                sseChunkCount++;
                yield return CreateSseChunk(content: chunk.Content);
            }
        }

        logger.LogInformation(
            "GetMessageStreamAsync completed: sessionId={SessionId}, sseChunks={SseChunkCount}",
            chatMessage.SessionId,
            sseChunkCount);
    }

    private static object CreateSseChunk(string? thinking = null, string? content = null) =>
        new
        {
            id = $"chatcmpl-{Guid.NewGuid()}",
            @object = ChatCompletionObject,
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = AgentModelName,
            choices = new[]
            {
                new
                {
                    index = 0,
                    delta = new { thinking, content },
                    finish_reason = (string?)null,
                },
            },
        };

    /// <summary>
    /// Gets the complete chat history for a specific agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <returns>A collection of chat history entries for the agent.</returns>
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(Guid agentId) =>
        await databaseResourceAccess.GetChatHistoryAsync(agentId);

    /// <summary>
    /// Gets the chat history for a specific session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the chat session.</param>
    /// <returns>A collection of chat history entries for the session.</returns>
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionAsync(string sessionId) =>
        await databaseResourceAccess.GetChatHistoryBySessionAsync(sessionId);

    /// <summary>
    /// Gets chat history entries based on specified filter criteria.
    /// </summary>
    /// <param name="filter">The filter criteria to apply to the chat history search.</param>
    /// <returns>A collection of chat history entries matching the filter criteria.</returns>
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryWithFilterAsync(ChatHistoryFilter filter) =>
        await databaseResourceAccess.GetChatHistoryWithFilterAsync(filter);

    /// <summary>
    /// Gets a comprehensive summary of chat history statistics for a specific agent or all agents.
    /// </summary>
    /// <param name="agentId">Optional agent ID to filter by. If null, returns summary for all agents.</param>
    /// <returns>A ChatHistorySummary containing total messages, sessions, tokens, and breakdowns by agent, role, and date.</returns>
    public async Task<ChatHistorySummary> GetChatHistorySummaryAsync(Guid? agentId = null) =>
        await databaseResourceAccess.GetChatHistorySummaryAsync(agentId);

    /// <summary>
    /// Gets chat history for a specific agent, filtered by user.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of chat history entries for the agent and user.</returns>
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryForUserAsync(Guid agentId, Guid userId) =>
        await databaseResourceAccess.GetChatHistoryForUserAsync(agentId, userId);

    /// <summary>
    /// Gets chat history for a specific session, filtered by user.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the chat session.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of chat history entries for the session and user.</returns>
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionForUserAsync(string sessionId, Guid userId) =>
        await databaseResourceAccess.GetChatHistoryBySessionForUserAsync(sessionId, userId);

    /// <summary>
    /// Gets all chat sessions, optionally filtered by agent ID.
    /// </summary>
    /// <param name="agentId">Optional agent ID to filter sessions by.</param>
    /// <returns>A collection of chat sessions with basic information.</returns>
    public async Task<IEnumerable<ChatSession>> GetChatSessionsAsync(Guid? agentId = null) =>
        await databaseResourceAccess.GetChatSessionsAsync(agentId);

    /// <summary>
    /// Gets a specific chat session by its unique identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the chat session.</param>
    /// <returns>A ChatSession object containing session details.</returns>
    public async Task<ChatSession> GetChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.GetChatSessionAsync(sessionId);

    /// <summary>
    /// Gets all chat sessions for a specific user, optionally filtered by agent ID.
    /// </summary>
    /// <param name="agentId">Optional agent ID to filter sessions by.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of chat sessions for the user.</returns>
    public async Task<IEnumerable<ChatSession>> GetChatSessionsForUserAsync(Guid? agentId, Guid userId) =>
        await databaseResourceAccess.GetChatSessionsForUserAsync(agentId, userId);

    /// <summary>
    /// Gets a specific chat session by its unique identifier if owned by the user.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the chat session.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A ChatSession object containing session details, or null if not found or not accessible.</returns>
    public async Task<ChatSession?> GetChatSessionForUserAsync(string sessionId, Guid userId) =>
        await databaseResourceAccess.GetChatSessionForUserAsync(sessionId, userId);

    /// <summary>
    /// Gets a detailed summary of a specific chat session including message count, tokens, and recent activity.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the chat session.</param>
    /// <returns>A ChatSessionSummary containing session details, message statistics, and recent message previews.</returns>
    public async Task<ChatSessionSummary> GetChatSessionSummaryAsync(string sessionId) =>
        await databaseResourceAccess.GetChatSessionSummaryAsync(sessionId);

    /// <summary>
    /// Creates a new chat session for the specified agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="sessionTitle">Optional title for the new session.</param>
    /// <returns>The unique identifier of the newly created session.</returns>
    public async Task<string> CreateNewChatSessionAsync(Guid agentId, string? sessionTitle = null) =>
        await databaseResourceAccess.CreateNewChatSessionAsync(agentId, sessionTitle);

    /// <summary>
    /// Creates a new chat session for the specified agent and user.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="sessionTitle">Optional title for the new session.</param>
    /// <returns>The unique identifier of the newly created session.</returns>
    public async Task<string> CreateNewChatSessionForUserAsync(Guid agentId, Guid userId, string? sessionTitle = null) =>
        await databaseResourceAccess.CreateNewChatSessionForUserAsync(agentId, userId, sessionTitle);

    /// <summary>
    /// Closes an active chat session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to close.</param>
    /// <returns>True if the session was successfully closed, otherwise false.</returns>
    public async Task<bool> CloseChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.CloseChatSessionAsync(sessionId);

    /// <summary>
    /// Permanently deletes a chat session and all its associated messages.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <returns>True if the session was successfully deleted, otherwise false.</returns>
    public async Task<bool> DeleteChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.DeleteChatSessionAsync(sessionId);

    /// <summary>
    /// Renames a chat session with a new title.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to rename.</param>
    /// <param name="newTitle">The new title for the session.</param>
    /// <returns>True if the session was successfully renamed, otherwise false.</returns>
    public async Task<bool> RenameChatSessionAsync(string sessionId, string newTitle) =>
        await databaseResourceAccess.RenameChatSessionAsync(sessionId, newTitle);

    /// <summary>
    /// Saves a chat message to the database.
    /// </summary>
    /// <param name="messageDto">The chat message to save.</param>
    /// <returns>The saved chat history entry with generated ID and timestamp.</returns>
    public async Task<ChatHistory> SaveChatMessageAsync(ChatMessage messageDto) =>
        await databaseResourceAccess.SaveChatMessageAsync(messageDto);

    /// <summary>
    /// Deletes a specific chat message by its unique identifier.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to delete.</param>
    /// <returns>True if the message was successfully deleted, otherwise false.</returns>
    public async Task<bool> DeleteChatMessageAsync(Guid messageId) =>
        await databaseResourceAccess.DeleteChatMessageAsync(messageId);

    /// <summary>
    /// Updates the content of an existing chat message.
    /// </summary>
    /// <param name="messageId">The unique identifier of the message to update.</param>
    /// <param name="newContent">The new content for the message.</param>
    /// <returns>True if the message was successfully updated, otherwise false.</returns>
    public async Task<bool> UpdateChatMessageAsync(Guid messageId, string newContent) =>
        await databaseResourceAccess.UpdateChatMessageAsync(messageId, newContent);

    /// <summary>
    /// Searches for chat messages containing the specified search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for in message content.</param>
    /// <param name="agentId">Optional agent ID to limit the search scope.</param>
    /// <returns>A collection of chat history entries matching the search criteria.</returns>
    public async Task<IEnumerable<ChatHistory>> SearchChatMessagesAsync(string searchTerm, Guid? agentId = null) =>
        await databaseResourceAccess.SearchChatMessagesAsync(searchTerm, agentId);

    /// <summary>
    /// Gets the count of messages grouped by agent within an optional date range.
    /// </summary>
    /// <param name="fromDate">Optional start date for filtering messages.</param>
    /// <param name="toDate">Optional end date for filtering messages.</param>
    /// <returns>A dictionary mapping agent names to their message counts.</returns>
    public async Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await databaseResourceAccess.GetMessageCountByAgentAsync(fromDate, toDate);

    /// <summary>
    /// Gets the count of messages grouped by role (user/assistant) within an optional date range.
    /// </summary>
    /// <param name="fromDate">Optional start date for filtering messages.</param>
    /// <param name="toDate">Optional end date for filtering messages.</param>
    /// <returns>A dictionary mapping roles to their message counts.</returns>
    public async Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await databaseResourceAccess.GetMessageCountByRoleAsync(fromDate, toDate);

    /// <summary>
    /// Gets the count of messages grouped by date within an optional date range.
    /// </summary>
    /// <param name="fromDate">Optional start date for filtering messages.</param>
    /// <param name="toDate">Optional end date for filtering messages.</param>
    /// <returns>A dictionary mapping dates to their message counts.</returns>
    public async Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await databaseResourceAccess.GetMessageCountByDateAsync(fromDate, toDate);

    /// <summary>
    /// Gets the total token usage across all messages within an optional date range.
    /// </summary>
    /// <param name="fromDate">Optional start date for filtering messages.</param>
    /// <param name="toDate">Optional end date for filtering messages.</param>
    /// <returns>The total number of tokens used in the specified date range.</returns>
    public async Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await databaseResourceAccess.GetTotalTokenUsageAsync(fromDate, toDate);

    /// <summary>
    /// Removes chat history entries older than the specified number of days.
    /// </summary>
    /// <param name="daysToKeep">The number of days of history to retain.</param>
    /// <returns>True if cleanup was successful, otherwise false.</returns>
    public async Task<bool> CleanupOldChatHistoryAsync(int daysToKeep) =>
        await databaseResourceAccess.CleanupOldChatHistoryAsync(daysToKeep);

    /// <summary>
    /// Archives a chat session, marking it as inactive but preserving its data.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to archive.</param>
    /// <returns>True if the session was successfully archived, otherwise false.</returns>
    public async Task<bool> ArchiveChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.ArchiveChatSessionAsync(sessionId);

    /// <summary>
    /// Exports a chat session to the specified format.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to export.</param>
    /// <param name="format">The export format (e.g., "json", "csv").</param>
    /// <returns>True if the export was successful, otherwise false.</returns>
    public async Task<bool> ExportChatSessionAsync(string sessionId, string format = "json") =>
        await databaseResourceAccess.ExportChatSessionAsync(sessionId, format);

    /// <summary>
    /// Gets the currently active prompt for an agent.
    /// </summary>
    /// <param name="agent">The agent to get the active prompt for.</param>
    /// <returns>The active prompt if found, otherwise null.</returns>
    public Prompt? GetActivePrompt(Agent agent) => agent.Prompts.FirstOrDefault(p => p.IsActive);

    /// <summary>
    /// Validates a chat message for required fields and constraints.
    /// </summary>
    /// <param name="chatMessage">The chat message to validate.</param>
    /// <returns>A validation result indicating success or failure with error message.</returns>
    private static (bool IsValid, string ErrorMessage) ValidateChatMessage(ChatMessage chatMessage)
    {
        if (!chatMessage.AgentId.HasValue)
        {
            return (false, "Error: Agent ID is required");
        }

        if (string.IsNullOrWhiteSpace(chatMessage.Content))
        {
            return (false, "Error: Message content is required");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Saves a user message to the database.
    /// </summary>
    /// <param name="chatMessage">The original chat message.</param>
    /// <param name="sessionId">The session ID to associate with the message.</param>
    private async Task SaveUserMessageAsync(ChatMessage chatMessage, string sessionId)
    {
        await databaseResourceAccess.SaveChatMessageAsync(new ChatMessage
        {
            Content = chatMessage.Content,
            Role = UserRole,
            AgentId = chatMessage.AgentId!.Value,
            SessionId = sessionId,
            UserId = chatMessage.UserId,
            Images = chatMessage.Images
        });
    }

    /// <summary>
    /// Saves an assistant message to the database.
    /// </summary>
    /// <param name="agent">The agent that generated the response.</param>
    /// <param name="sessionId">The session ID to associate with the message.</param>
    /// <param name="responseContent">The response content chunks.</param>
    /// <param name="thinkingContent">The thinking content chunks.</param>
    /// <param name="toolResults"></param>
    private async Task SaveAssistantMessageAsync(Agent agent, string sessionId, List<string?> responseContent,
        List<string?> thinkingContent, List<ToolExecutionLog> toolResults)
    {
        var fullResponse = string.Join("", responseContent);
        var fullThinking = string.Join("", thinkingContent);
        var allExecutions = toolResults.ToList();
        var persistableExecutions = allExecutions.Where(e => e.ToolId != Guid.Empty).ToList();

        if (string.IsNullOrWhiteSpace(fullResponse))
        {
            if (!allExecutions.Any())
            {
                logger.LogDebug(
                    "Skipping save of empty assistant message for sessionId={SessionId}",
                    sessionId);
                return;
            }

            fullResponse = string.Join(
                "\n",
                allExecutions.Select(e => e.Success
                    ? $"[{e.ToolName}] {e.Result}"
                    : $"[{e.ToolName}] Error: {e.ErrorMessage ?? e.Result}"));
        }

        await databaseResourceAccess.SaveChatMessageAsync(new ChatMessage
        {
            Content = fullResponse,
            Role = AssistantRole,
            AgentId = agent.Id,
            SessionId = sessionId,
            TokenCount = fullResponse.Length * DefaultTokenCountMultiplier,
            ToolResults = persistableExecutions,
            IsToolExecution = persistableExecutions.Any(),
            Thinking = string.IsNullOrEmpty(fullThinking) ? null : fullThinking
        });
    }


    /// <summary>
    /// Processes tool calls and returns follow-up response chunks.
    /// </summary>
    /// <param name="agent">The agent processing the tools.</param>
    /// <param name="activePrompt">The active prompt for the agent.</param>
    /// <param name="sessionId">The session ID for the conversation.</param>
    /// <param name="toolsToCall">The tools to execute.</param>
    /// <returns>An async enumerable of response chunks from tool execution.</returns>
    private async IAsyncEnumerable<StreamingResult> ProcessToolCallsAsync(
        Agent agent,
        Prompt activePrompt,
        string sessionId,
        List<ToolCall?> toolsToCall)
    {
        var toolResults = await toolExecutionService.ExecuteToolCallsAsync(toolsToCall.Where(t => t != null).Cast<ToolCall>().ToList(), agent, sessionId);
        
  
        var followUpMessage = toolExecutionService.CreateFollowUpMessage(toolResults.Select(FormFollowMessage()).ToList());

        await foreach (var chunk in SendMessageStreamRecursiveAsync(agent, activePrompt, followUpMessage, sessionId, toolResults))
        {
            yield return chunk;
        }
    }

    private static Func<ToolExecutionLog, string> FormFollowMessage() => toolResult => toolResult.Success
        ? $"Tool {toolResult.ToolName}: {toolResult.Result}"
        : $"Tool {toolResult.ToolName}: Error - {toolResult.ErrorMessage}";

   

    /// <summary>
    /// Extracts the tool name from a tool result message.
    /// </summary>
    /// <param name="toolResult">The tool result message.</param>
    /// <returns>The extracted tool name or null if not found.</returns>
    private string? ExtractToolNameFromResult(string toolResult)
    {
        // Tool result format: "Tool {ToolName}: {Result}" or "Tool {ToolName}: Error - {ErrorMessage}"
        var match = System.Text.RegularExpressions.Regex.Match(toolResult, @"Tool\s+([^:]+):");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    /// <summary>
    /// Extracts the result content from a tool result message.
    /// </summary>
    /// <param name="toolResult">The tool result message.</param>
    /// <returns>The extracted result content.</returns>
    private string ExtractResultFromToolResult(string toolResult)
    {
        // Tool result format: "Tool {ToolName}: {Result}" or "Tool {ToolName}: Error - {ErrorMessage}"
        var match = System.Text.RegularExpressions.Regex.Match(toolResult, @"Tool\s+[^:]+:\s*(.+)");
        return match.Success ? match.Groups[1].Value.Trim() : toolResult;
    }
}
