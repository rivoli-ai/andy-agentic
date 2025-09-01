using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using AutoMapper;
using System.Text.Json;
using static Andy.Agentic.Application.Services.ChatService;

namespace Andy.Agentic.Application.Services;

public class ChatService(
    IDataBaseService databaseResourceAccess,
    ILlmService llmResourceAccess,
    IToolExecutionService toolExecutionService)
    : IChatService
{
    public async IAsyncEnumerable<string> SendMessageStreamAsync(ChatMessage chatMessage)
    {
        if (!chatMessage.AgentId.HasValue)
        {
            yield return "Error: Agent ID is required";
            yield break;
        }

        var agent = await databaseResourceAccess.GetAgentByIdAsync(chatMessage.AgentId.Value);
        if (agent == null)
        {
            yield return "Error: Agent not found";
            yield break;
        }

        var activePrompt = GetActivePrompt(agent);

        if (activePrompt == null)
        {
            yield return "Error: Agent has no active prompt";
            yield break;
        }

        var sessionId = chatMessage.SessionId ??
                        await databaseResourceAccess.CreateNewChatSessionAsync(chatMessage.AgentId.Value);

        await databaseResourceAccess.SaveChatMessageAsync(new ChatMessage
        {
            Content = chatMessage.Content, Role = "user", AgentId = chatMessage.AgentId.Value, SessionId = sessionId
        });

        await foreach (var chunk in
                       SendMessageStreamRecursiveAsync(agent, activePrompt, chatMessage.Content, sessionId))
        {
            yield return chunk;
        }
    }


    public async IAsyncEnumerable<string> SendMessageStreamRecursiveAsync(
        Agent agent,
        Prompt activePrompt,
        string messageContent,
        string sessionId)
    {
        var recentMessages = await databaseResourceAccess.GetChatHistoryBySessionAsync(sessionId);


        var (llmMessage, tools) = await llmResourceAccess.PrepareLlmMessageAsync(
            agent,
            activePrompt,
            messageContent,
            sessionId,
            recentMessages.ToList()
        );
        var toolCalls = new List<ToolCall>();
        var responseContent = new List<string>();

        await foreach (var chunk in llmResourceAccess.SendToLlmProviderStreamAsync(agent.LlmConfig, llmMessage, tools,
                           toolCalls))
        {
            responseContent.Add(chunk);
            yield return chunk;
        }

        if (responseContent.Any())
        {
            var fullResponse = string.Join("", responseContent);
            await databaseResourceAccess.SaveChatMessageAsync(new ChatMessage
            {
                Content = fullResponse,
                Role = "assistant",
                AgentId = agent.Id,
                SessionId = sessionId,
                TokenCount = fullResponse.Length
            });
        }

        if (!toolCalls.Any())
        {
            yield break;
        }

        var toolResults = await toolExecutionService.ExecuteToolCallsAsync(toolCalls, agent, sessionId);
        var followUpMessage = toolExecutionService.CreateFollowUpMessage(llmMessage, toolResults);

        await foreach (var chunk in SendMessageStreamRecursiveAsync(agent, activePrompt, followUpMessage, sessionId))
        {
            yield return chunk;
        }
    }


    public async Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(Guid agentId) =>
        await databaseResourceAccess.GetChatHistoryAsync(agentId);

    public async Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionAsync(string sessionId) =>
        await databaseResourceAccess.GetChatHistoryBySessionAsync(sessionId);


    public async Task<IEnumerable<ChatHistory>> GetChatHistoryWithFilterAsync(ChatHistoryFilter filter) =>
        await databaseResourceAccess.GetChatHistoryWithFilterAsync(filter);

    public async Task<ChatHistorySummary> GetChatHistorySummaryAsync(Guid? agentId = null) =>
        await databaseResourceAccess.GetChatHistorySummaryAsync(agentId);

    public async Task<IEnumerable<ChatSession>> GetChatSessionsAsync(Guid? agentId = null) =>
        await databaseResourceAccess.GetChatSessionsAsync(agentId);

    public async Task<ChatSession> GetChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.GetChatSessionAsync(sessionId);

    public async Task<ChatSessionSummary> GetChatSessionSummaryAsync(string sessionId) =>
        await databaseResourceAccess.GetChatSessionSummaryAsync(sessionId);

    public async Task<string> CreateNewChatSessionAsync(Guid agentId, string? sessionTitle = null) =>
        await databaseResourceAccess.CreateNewChatSessionAsync(agentId, sessionTitle);

    public async Task<bool> CloseChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.CloseChatSessionAsync(sessionId);

    public async Task<bool> DeleteChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.DeleteChatSessionAsync(sessionId);

    public async Task<bool> RenameChatSessionAsync(string sessionId, string newTitle) =>
        await databaseResourceAccess.RenameChatSessionAsync(sessionId, newTitle);

    public async Task<ChatHistory> SaveChatMessageAsync(ChatMessage messageDto) =>
        await databaseResourceAccess.SaveChatMessageAsync(messageDto);

    public async Task<bool> DeleteChatMessageAsync(Guid messageId) =>
        await databaseResourceAccess.DeleteChatMessageAsync(messageId);

    public async Task<bool> UpdateChatMessageAsync(Guid messageId, string newContent) =>
        await databaseResourceAccess.UpdateChatMessageAsync(messageId, newContent);

    public async Task<IEnumerable<ChatHistory>> SearchChatMessagesAsync(string searchTerm, Guid? agentId = null) =>
        await databaseResourceAccess.SearchChatMessagesAsync(searchTerm, agentId);

    public async Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await databaseResourceAccess.GetMessageCountByAgentAsync(fromDate, toDate);

    public async Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await databaseResourceAccess.GetMessageCountByRoleAsync(fromDate, toDate);

    public async Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await databaseResourceAccess.GetMessageCountByDateAsync(fromDate, toDate);

    public async Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await databaseResourceAccess.GetTotalTokenUsageAsync(fromDate, toDate);

    public async Task<bool> CleanupOldChatHistoryAsync(int daysToKeep) =>
        await databaseResourceAccess.CleanupOldChatHistoryAsync(daysToKeep);

    public async Task<bool> ArchiveChatSessionAsync(string sessionId) =>
        await databaseResourceAccess.ArchiveChatSessionAsync(sessionId);

    public async Task<bool> ExportChatSessionAsync(string sessionId, string format = "json") =>
        await databaseResourceAccess.ExportChatSessionAsync(sessionId, format);

    public Prompt? GetActivePrompt(Agent agent) => agent.Prompts.FirstOrDefault(p => p.IsActive);

    public async IAsyncEnumerable<object> GetMessageStreamAsync(ChatMessage chatMessage)
    {
        await foreach (var chunk in SendMessageStreamAsync(chatMessage))
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                yield return new
                {
                    id = $"chatcmpl-{Guid.NewGuid()}",
                    @object = "chat.completion.chunk",
                    created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    model = "agent-model",
                    choices = new[]
                    {
                        new
                        {
                            index = 0,
                            delta = new { content = chunk },
                            finish_reason = (string?)null
                        }
                    }
                };
            }
        }
    }
}
