using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Queries.SearchCriteria;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides operations for storing and querying chat messages and sessions.
/// </summary>
public interface IChatRepository
{
    /// <summary>
    ///     Gets recent chat history for an agent.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(Guid agentId, int maxCount = 50);

    /// <summary>
    ///     Persists a chat message.
    /// </summary>
    Task<ChatMessageEntity> SaveMessageAsync(ChatMessageEntity message);

    /// <summary>
    ///     Gets messages for a specific session.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetBySessionAsync(string sessionId);

    /// <summary>
    ///     Searches messages with optional agent filter.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> SearchMessagesAsync(string query, Guid? agentId = null);

    /// <summary>
    ///     Retrieves a message by its identifier.
    /// </summary>
    Task<ChatMessageEntity?> GetMessageByIdAsync(Guid messageId);

    /// <summary>
    ///     Deletes a message by its identifier.
    /// </summary>
    Task<bool> DeleteMessageAsync(Guid messageId);

    /// <summary>
    ///     Deletes a chat session and its messages.
    /// </summary>
    Task<bool> DeleteSessionAsync(string sessionId);

    /// <summary>
    ///     Gets recent chat history for a session with a limit.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionIdAsync(string sessionId, int maxCount = 50);

    /// <summary>
    ///     Gets chat history using a filter criteria object.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(ChatHistoryFilter filter);

    /// <summary>
    ///     Gets all chat history optionally filtered by agent.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(Guid? agentId = null);

    /// <summary>
    ///     Gets chat history for a specific session.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionId(string sessionId);

    /// <summary>
    ///     Gets chat history for a specific agent, filtered by user.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryForUserAsync(Guid? agentId, Guid userId);

    /// <summary>
    ///     Gets chat history for a specific session, filtered by user.
    /// </summary>
    Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionForUserAsync(string sessionId, Guid userId);

    // New methods for chat management
    /// <summary>
    ///     Creates and returns a new session identifier for the given agent.
    /// </summary>
    Task<string> CreateNewSessionAsync(Guid agentId, string? sessionTitle = null);

    /// <summary>
    ///     Creates and returns a new session identifier for the given agent and user.
    /// </summary>
    Task<string> CreateNewSessionForUserAsync(Guid agentId, Guid userId, string? sessionTitle = null);

    /// <summary>
    ///     Closes a session and prevents further messages.
    /// </summary>
    Task<bool> CloseSessionAsync(string sessionId);

    /// <summary>
    ///     Renames a session.
    /// </summary>
    Task<bool> RenameSessionAsync(string sessionId, string newTitle);

    /// <summary>
    ///     Archives a session.
    /// </summary>
    Task<bool> ArchiveSessionAsync(string sessionId);

    /// <summary>
    ///     Updates a message's content.
    /// </summary>
    Task<bool> UpdateMessageAsync(Guid messageId, string newContent);

    // Analytics methods
    /// <summary>
    ///     Gets message counts grouped by agent within an optional date range.
    /// </summary>
    Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    ///     Gets message counts grouped by role within an optional date range.
    /// </summary>
    Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    ///     Gets message counts grouped by date within an optional range.
    /// </summary>
    Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    ///     Calculates total token usage within an optional date range.
    /// </summary>
    Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    ///     Deletes historical data older than the specified number of days.
    /// </summary>
    Task<bool> CleanupOldHistoryAsync(int daysToKeep);
}
