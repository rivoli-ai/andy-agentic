using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository implementation for managing ChatMessage entities and chat history in the database.
    /// Provides comprehensive chat functionality including message storage, retrieval by various criteria,
    /// session management, filtering, sorting, and pagination. Supports advanced search capabilities
    /// for chat history with multiple filter options and sorting preferences.
    /// </summary>
    public class ChatRepository(AndyDbContext context) : IChatRepository
    {
        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(Guid agentId, int maxCount = 50)
        {
            return await context.ChatMessages
                .Where(ch => ch.AgentId == agentId)
                .OrderByDescending(ch => ch.Timestamp)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionIdAsync(string sessionId, int maxCount = 50)
        {
           return await context.ChatMessages
                .Where(ch => ch.SessionId == sessionId)
                .Include(x=>x.ToolResults)
                .ThenInclude(x=>x.Tool)
                .OrderBy(ch => ch.Timestamp)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionId(string sessionId)
        {
            return context.ChatMessages   .Where(ch => ch.SessionId == sessionId);
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(Guid? agentId = null)
        {
            var query = context.ChatMessages.AsQueryable();

            if (agentId.HasValue)
                query = query.Where(ch => ch.AgentId == agentId.Value);

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryAsync(ChatHistoryFilter filter)
        {
            var query = context.ChatMessages.AsQueryable();

            if (filter.AgentId.HasValue)
                query = query.Where(ch => ch.AgentId == filter.AgentId.Value);

            if (!string.IsNullOrEmpty(filter.SessionId))
                query = query.Where(ch => ch.SessionId == filter.SessionId);

            if (filter.FromDate.HasValue)
                query = query.Where(ch => ch.Timestamp >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(ch => ch.Timestamp <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(ch => ch.Content.Contains(filter.SearchTerm));

            if (!string.IsNullOrEmpty(filter.Role))
                query = query.Where(ch => ch.Role == filter.Role);

            if (filter.IsToolExecution.HasValue)
                query = query.Where(ch => ch.IsToolExecution == filter.IsToolExecution.Value);

            query = filter.SortBy.ToLower() switch
            {
                "timestamp" => filter.SortDescending
                    ? query.OrderByDescending(ch => ch.Timestamp)
                    : query.OrderBy(ch => ch.Timestamp),
                "content" => filter.SortDescending
                    ? query.OrderByDescending(ch => ch.Content)
                    : query.OrderBy(ch => ch.Content),
                "role" => filter.SortDescending ? query.OrderByDescending(ch => ch.Role) : query.OrderBy(ch => ch.Role),
                _ => filter.SortDescending ? query.OrderByDescending(ch => ch.Timestamp) : query.OrderBy(ch => ch.Timestamp)
            };


            var skip = (filter.Page - 1) * filter.PageSize;
            query = query.Skip(skip).Take(filter.PageSize);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryForUserAsync(Guid? agentId, Guid userId)
        {
            return await context.ChatMessages
                .Where(ch => ch.AgentId == agentId && ch.UserId == userId)
                .OrderByDescending(ch => ch.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetHistoryBySessionForUserAsync(string sessionId, Guid userId)
        {
            return await context.ChatMessages
                .Where(ch => ch.SessionId == sessionId && ch.UserId == userId)
                .Include(x => x.ToolResults)
                .ThenInclude(x => x.Tool)
                .OrderBy(ch => ch.Timestamp)
                .ToListAsync();
        }

        public async Task<ChatMessageEntity> SaveMessageAsync(ChatMessageEntity message)
        {
            if (message.Id == Guid.Empty)
            {
                message.Id = Guid.NewGuid();
            }
            
            if (message.Timestamp == default)
            {
                message.Timestamp = DateTime.UtcNow;
            }

            context.ChatMessages.Add(message);
            await context.SaveChangesAsync();
            return message;
        }

        public async Task<IEnumerable<ChatMessageEntity>> GetBySessionAsync(string sessionId)
        {
            return await context.ChatMessages
                .Where(cm => cm.SessionId == sessionId)
                .OrderBy(cm => cm.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessageEntity>> SearchMessagesAsync(string query, Guid? agentId = null)
        {
            var queryable = context.ChatMessages.AsQueryable();
            
            if (agentId.HasValue)
            {
                queryable = queryable.Where(cm => cm.AgentId == agentId.Value);
            }

            return await queryable
                .Where(cm => cm.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(cm => cm.Timestamp)
                .Take(100)
                .ToListAsync();
        }

        public async Task<ChatMessageEntity?> GetMessageByIdAsync(Guid messageId)
        {
            return await context.ChatMessages.FindAsync(messageId);
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId)
        {
            var message = await context.ChatMessages.FindAsync(messageId);
            if (message == null)
                return false;

            context.ChatMessages.Remove(message);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            var messages = await context.ChatMessages
                .Where(cm => cm.SessionId == sessionId)
                .ToListAsync();

            if (!messages.Any())
                return false;

            context.ChatMessages.RemoveRange(messages);
            await context.SaveChangesAsync();
            return true;
        }

        // New methods for chat management
        public async Task<string> CreateNewSessionAsync(Guid agentId, string? sessionTitle = null)
        {
            var sessionId = Guid.NewGuid().ToString();

            // Create a session marker message
            var sessionMessage = new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                Content = sessionTitle ?? "New Chat Session",
                Role = "system",
                AgentId = agentId,
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow,
                IsToolExecution = false
            };

            context.ChatMessages.Add(sessionMessage);
            await context.SaveChangesAsync();

            return sessionId;
        }

        public async Task<string> CreateNewSessionForUserAsync(Guid agentId, Guid userId, string? sessionTitle = null)
        {
            var sessionId = Guid.NewGuid().ToString();

            // Create a session marker message with user ID
            var sessionMessage = new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                Content = sessionTitle ?? "New Chat Session",
                Role = "system",
                AgentId = agentId,
                SessionId = sessionId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                IsToolExecution = false
            };

            context.ChatMessages.Add(sessionMessage);
            await context.SaveChangesAsync();

            return sessionId;
        }

        public async Task<bool> CloseSessionAsync(string sessionId)
        {
            // Mark session as inactive by updating the last message
            var lastMessage = await context.ChatMessages
                .Where(ch => ch.SessionId == sessionId)
                .OrderByDescending(ch => ch.Timestamp)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                lastMessage.Timestamp = DateTime.UtcNow; // Update timestamp to mark session closure
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> RenameSessionAsync(string sessionId, string newTitle)
        {
            var sessionMessage = await context.ChatMessages
                .Where(ch => ch.SessionId == sessionId && ch.Role == "system")
                .FirstOrDefaultAsync();

            if (sessionMessage != null)
            {
                sessionMessage.Content = newTitle;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> ArchiveSessionAsync(string sessionId)
        {
            // Mark session as archived by updating the session message
            var sessionMessage = await context.ChatMessages
                .Where(ch => ch.SessionId == sessionId && ch.Role == "system")
                .FirstOrDefaultAsync();

            if (sessionMessage != null)
            {
                sessionMessage.Content = "[ARCHIVED] " + sessionMessage.Content;
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateMessageAsync(Guid messageId, string newContent)
        {
            var message = await context.ChatMessages.FindAsync(messageId);
            if (message != null)
            {
                message.Content = newContent;
                message.Timestamp = DateTime.UtcNow; // Update timestamp to show it was modified
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Analytics methods
        public async Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = context.ChatMessages.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ch => ch.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ch => ch.Timestamp <= toDate.Value);

            return await query
                .GroupBy(ch => ch.AgentName)
                .Select(g => new { AgentName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AgentName, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = context.ChatMessages.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ch => ch.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ch => ch.Timestamp <= toDate.Value);

            return await query
                .GroupBy(ch => ch.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);
        }

        public async Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = context.ChatMessages.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ch => ch.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ch => ch.Timestamp <= toDate.Value);

            return await query
                .GroupBy(ch => ch.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);
        }

        public async Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = context.ChatMessages.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(ch => ch.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(ch => ch.Timestamp <= toDate.Value);

            return await query.SumAsync(ch => ch.TokenCount ?? 0);
        }

        public async Task<bool> CleanupOldHistoryAsync(int daysToKeep)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldMessages = await context.ChatMessages
                .Where(ch => ch.Timestamp < cutoffDate)
                .ToListAsync();

            if (oldMessages.Any())
            {
                context.ChatMessages.RemoveRange(oldMessages);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
