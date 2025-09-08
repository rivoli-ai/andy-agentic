using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;

namespace Andy.Agentic.Application.Interfaces;

public interface IChatService
{
    IAsyncEnumerable<object> GetMessageStreamAsync(ChatMessage chatMessage);

    IAsyncEnumerable<string> SendMessageStreamAsync(ChatMessage chatMessage);
    
    Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(Guid agentId);
    Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionAsync(string sessionId);
    Task<IEnumerable<ChatHistory>> GetChatHistoryWithFilterAsync(ChatHistoryFilter filter);
    Task<ChatHistorySummary> GetChatHistorySummaryAsync(Guid? agentId = null);

    // User-specific Chat History Management
    Task<IEnumerable<ChatHistory>> GetChatHistoryForUserAsync(Guid agentId, Guid userId);
    Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionForUserAsync(string sessionId, Guid userId);

    // Chat Sessions Management
    Task<IEnumerable<ChatSession>> GetChatSessionsAsync(Guid? agentId = null);
    Task<ChatSession> GetChatSessionAsync(string sessionId);
    Task<IEnumerable<ChatSession>> GetChatSessionsForUserAsync(Guid? agentId, Guid userId);
    Task<ChatSession?> GetChatSessionForUserAsync(string sessionId, Guid userId);
    Task<ChatSessionSummary> GetChatSessionSummaryAsync(string sessionId);
    Task<string> CreateNewChatSessionAsync(Guid agentId, string? sessionTitle = null);
    Task<string> CreateNewChatSessionForUserAsync(Guid agentId, Guid userId, string? sessionTitle = null);
    Task<bool> CloseChatSessionAsync(string sessionId);
    Task<bool> DeleteChatSessionAsync(string sessionId);
    Task<bool> RenameChatSessionAsync(string sessionId, string newTitle);

    // Chat Message Management
    Task<ChatHistory> SaveChatMessageAsync(ChatMessage message);
    Task<bool> DeleteChatMessageAsync(Guid messageId);
    Task<bool> UpdateChatMessageAsync(Guid messageId, string newContent);
    Task<IEnumerable<ChatHistory>> SearchChatMessagesAsync(string searchTerm, Guid? agentId = null);

    // Analytics and Insights
    Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null);

    // Cleanup and Maintenance
    Task<bool> CleanupOldChatHistoryAsync(int daysToKeep);
    Task<bool> ArchiveChatSessionAsync(string sessionId);
    Task<bool> ExportChatSessionAsync(string sessionId, string format = "json");
    Prompt? GetActivePrompt(Agent agent);

    IAsyncEnumerable<string> SendMessageStreamRecursiveAsync(Agent agent, Prompt activePrompt,
        string messageContent, string sessionId,
        List<ToolExecutionLog>? toolResults = null);
}
