using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;

namespace Andy.Agentic.Domain.Interfaces.Database;

public interface IDataBaseService
{
    Task<IEnumerable<Agent>> GetAllAgentsAsync();
    Task<Agent?> GetAgentByIdAsync(Guid id);
    Task<Agent> CreateAgentAsync(Agent createAgent);
    Task<Agent> UpdateAgentAsync(Agent updateAgent);
    Task<bool> DeleteAgentAsync(Guid id);
    Task<IEnumerable<Agent>> SearchAgentsAsync(string query);
    Task<IEnumerable<Agent>> GetAgentsByTypeAsync(string type);
    Task<IEnumerable<Agent>> GetAgentsByTagAsync(string tag);
    Task<IEnumerable<Tool>> GetAllToolsAsync();
    Task<Tool?> GetToolByIdAsync(Guid id);
    Task<Tool> CreateToolAsync(Tool createTool);
    Task<Tool> UpdateToolAsync(Guid id, Tool updateTool);
    Task<bool> DeleteToolAsync(Guid id);
    Task<IEnumerable<Tool>> SearchToolsAsync(string query);
    Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category);
    Task<IEnumerable<Tool>> GetToolsByTypeAsync(string type);
    Task<IEnumerable<Tool>> GetActiveToolsAsync();
    Task<IEnumerable<LlmConfig>> GetAllLlmConfigsAsync();
    Task<LlmConfig?> GetLlmConfigByIdAsync(Guid id);
    Task<LlmConfig> CreateLlmConfigAsync(LlmConfig createLlmConfig);
    Task<LlmConfig> UpdateLlmConfigAsync(Guid id, LlmConfig updateLlmConfig);
    Task<bool> DeleteLlmConfigAsync(Guid id);
    Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(Guid agentId);
    Task<bool> DeleteSessionAsync(string sessionId);
    Task<ToolExecutionLog?> GetToolExecutionLogByIdAsync(Guid executionId);

    Task<ToolExecutionLog> LogToolExecutionAsync(ToolExecutionLog request, Tool? tool, object? result,
        bool success, string? errorMessage, long executionTime);

    Task<IEnumerable<ToolExecutionLog>> GetRecentToolExecutionsAsync(Guid? agentId, string sessionId);
    Task<IEnumerable<ToolExecutionLog>> GetRecentToolExecutionsAsync(int count);

    // Chat History Management
    Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionAsync(string sessionId);
    Task<IEnumerable<ChatHistory>> GetChatHistoryWithFilterAsync(ChatHistoryFilter filter);
    Task<ChatHistorySummary> GetChatHistorySummaryAsync(Guid? agentId = null);

    // Chat Sessions Management
    Task<IEnumerable<ChatSession>> GetChatSessionsAsync(Guid? agentId = null);
    Task<ChatSession> GetChatSessionAsync(string sessionId);
    Task<ChatSessionSummary> GetChatSessionSummaryAsync(string sessionId);
    Task<string> CreateNewChatSessionAsync(Guid agentId, string? sessionTitle = null);
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

    // Agent-specific database operations
    Task<Agent?> GetAgentWithConfigAsync(Guid agentId);
}
