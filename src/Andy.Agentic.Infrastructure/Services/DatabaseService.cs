using System.Text.Json;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using AutoMapper;

namespace Andy.Agentic.Infrastructure.Services;

public class DatabaseService(
    IAgentRepository agentRepository,
    IToolRepository toolRepository,
    ILlmRepository llmRepository,
    IChatRepository chatRepository,
    IToolExecutionRepository toolExecutionRepository,
    ITagRepository tagRepository,
    IUnitOfWork uow,
    IMapper mapper)
    : IDataBaseService
{
    public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
    {
        var agents = await agentRepository.GetAllAsync();
        return mapper.Map<IEnumerable<Agent>>(agents);
    }

    public async Task<Agent?> GetAgentByIdAsync(Guid id)
    {
        var agent = await agentRepository.GetByIdAsync(id);
        return agent != null ? mapper.Map<Agent>(agent) : null;
    }

    public async Task<Agent> CreateAgentAsync(Agent createAgent)
    {
        var agent = mapper.Map<AgentEntity>(createAgent);
        agent.CreatedAt = DateTime.UtcNow;
        agent.UpdatedAt = DateTime.UtcNow;

        var createdAgent = await agentRepository.CreateAsync(agent);
        return mapper.Map<Agent>(createdAgent);
    }

    public async Task<Agent> UpdateAgentAsync(Agent updateAgent)
    {
        var agent = mapper.Map<AgentEntity>(updateAgent);

        await agentRepository.UpdateAsync(agent);

        return mapper.Map<Agent>(agent);
    }

    public async Task<bool> DeleteAgentAsync(Guid id) => await agentRepository.DeleteAsync(id);

    public async Task<IEnumerable<Agent>> SearchAgentsAsync(string query)
    {
        var agents = await agentRepository.SearchAsync(query);
        return mapper.Map<IEnumerable<Agent>>(agents);
    }

    public async Task<IEnumerable<Agent>> GetAgentsByTypeAsync(string type)
    {
        var agents = await agentRepository.GetByTypeAsync(type);
        return mapper.Map<IEnumerable<Agent>>(agents);
    }

    public async Task<IEnumerable<Agent>> GetAgentsByTagAsync(string tag)
    {
        var agents = await agentRepository.GetByTagAsync(tag);
        return mapper.Map<IEnumerable<Agent>>(agents);
    }

    public async Task<IEnumerable<Tool>> GetAllToolsAsync()
    {
        var tools = await toolRepository.GetAllAsync();
        return mapper.Map<IEnumerable<Tool>>(tools);
    }

    public async Task<Tool?> GetToolByIdAsync(Guid id)
    {
        var tool = await toolRepository.GetByIdAsync(id);
        return tool != null ? mapper.Map<Tool>(tool) : null;
    }

    public async Task<Tool> CreateToolAsync(Tool createTool)
    {
        var tool = mapper.Map<ToolEntity>(createTool);
        var createol = await toolRepository.CreateAsync(tool);
        return mapper.Map<Tool>(createol);
    }

    public async Task<Tool> UpdateToolAsync(Guid id, Tool updateTool)
    {
        var existingTool = await toolRepository.GetByIdAsync(id);
        if (existingTool == null)
        {
            throw new ArgumentException($"Tool with ID {id} not found");
        }

        mapper.Map(updateTool, existingTool);
        var updateol = await toolRepository.UpdateAsync(existingTool);
        return mapper.Map<Tool>(updateol);
    }

    public async Task<bool> DeleteToolAsync(Guid id) => await toolRepository.DeleteAsync(id);

    public async Task<IEnumerable<Tool>> SearchToolsAsync(string query)
    {
        var tools = await toolRepository.SearchAsync(query);
        return mapper.Map<IEnumerable<Tool>>(tools);
    }

    public async Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category)
    {
        var tools = await toolRepository.GetByCategoryAsync(category);
        return mapper.Map<IEnumerable<Tool>>(tools);
    }

    public async Task<IEnumerable<Tool>> GetToolsByTypeAsync(string type)
    {
        var tools = await toolRepository.GetByTypeAsync(type);
        return mapper.Map<IEnumerable<Tool>>(tools);
    }

    public async Task<IEnumerable<Tool>> GetActiveToolsAsync()
    {
        var tools = await toolRepository.GetActiveAsync();
        return mapper.Map<IEnumerable<Tool>>(tools);
    }

    public async Task<IEnumerable<LlmConfig>> GetAllLlmConfigsAsync()
    {
        var configs = await llmRepository.GetAllAsync();
        return mapper.Map<IEnumerable<LlmConfig>>(configs);
    }

    public async Task<LlmConfig?> GetLlmConfigByIdAsync(Guid id)
    {
        var config = await llmRepository.GetByIdAsync(id);
        return config != null ? mapper.Map<LlmConfig>(config) : null;
    }

    public async Task<LlmConfig> CreateLlmConfigAsync(LlmConfig createLlmConfig)
    {
        var config = mapper.Map<LlmConfigEntity>(createLlmConfig);
        var createdConfig = await llmRepository.CreateAsync(config);
        return mapper.Map<LlmConfig>(createdConfig);
    }

    public async Task<LlmConfig> UpdateLlmConfigAsync(Guid id, LlmConfig updateLlmConfig)
    {
        var existingConfig = await llmRepository.GetByIdAsync(id);
        if (existingConfig == null)
        {
            throw new ArgumentException("LLM Config not found");
        }

        mapper.Map(updateLlmConfig, existingConfig);
        var updatedConfig = await llmRepository.UpdateAsync(existingConfig);
        return mapper.Map<LlmConfig>(updatedConfig);
    }

    public async Task<bool> DeleteLlmConfigAsync(Guid id) => await llmRepository.DeleteAsync(id);

    public async Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(Guid agentId)
    {
        var messages = await chatRepository.GetHistoryAsync(agentId);
        return mapper.Map<IEnumerable<ChatHistory>>(messages);
    }

    public async Task<bool> DeleteSessionAsync(string sessionId) => await chatRepository.DeleteSessionAsync(sessionId);

    public async Task<ToolExecutionLog> LogToolExecutionAsync(ToolExecutionLog request, Tool? tool,
        object? result, bool success, string? errorMessage, long executionTime)
    {
        var log = new ToolExecutionLogEntity
        {
            Id = Guid.NewGuid(),
            ToolId = request.ToolId,
            ToolName = tool?.Name ?? request.ToolName,
            AgentId = request.AgentId,
            SessionId = request.SessionId,
            Parameters = JsonSerializer.Serialize(request.Parameters),
            Result = result != null ? JsonSerializer.Serialize(result) : null,
            Success = success,
            ErrorMessage = errorMessage,
            ExecutedAt = DateTime.UtcNow,
            ExecutionTime = executionTime
        };

        var toolLogs = await toolExecutionRepository.LogExecutionAsync(log);
        return mapper.Map<ToolExecutionLog>(toolLogs);
    }

    public async Task<IEnumerable<ToolExecutionLog>> GetRecentToolExecutionsAsync(Guid? agentId, string sessionId)
    {
        var result = await toolExecutionRepository.GetRecentExecutionsAsync(agentId, sessionId);
        return mapper.Map<IEnumerable<ToolExecutionLog>>(result);
    }

    public async Task<IEnumerable<ToolExecutionLog>> GetRecentToolExecutionsAsync(int count)
    {
        var result = await toolExecutionRepository.GetRecentExecutionsAsync(count);
        return mapper.Map<IEnumerable<ToolExecutionLog>>(result);
    }

    public async Task<ToolExecutionLog?> GetToolExecutionLogByIdAsync(Guid executionId)
    {
        var log = await toolExecutionRepository.GetLogByIdAsync(executionId);
        if (log == null)
        {
            return null;
        }

        return new ToolExecutionLog
        {
            Id = log.Id,
            ToolId = log.ToolId,
            ToolName = log.ToolName,
            AgentId = log.AgentId,
            SessionId = log.SessionId,
            Parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(log.Parameters ?? "{}") ??
                         new Dictionary<string, object>(),
            Result = log.Result != null ? JsonSerializer.Deserialize<object>(log.Result) : null,
            Success = log.Success,
            ErrorMessage = log.ErrorMessage,
            ExecutedAt = log.ExecutedAt,
            ExecutionTime = log.ExecutionTime
        };
    }

    // Chat History Management
    public async Task<IEnumerable<ChatHistory>> GetChatHistoryBySessionAsync(string sessionId)
    {
        var messages = await chatRepository.GetHistoryBySessionIdAsync(sessionId);
        return mapper.Map<IEnumerable<ChatHistory>>(messages);
    }

    public async Task<IEnumerable<ChatHistory>> GetChatHistoryWithFilterAsync(ChatHistoryFilter filter)
    {
        var messages = await chatRepository.GetHistoryAsync(filter);
        return mapper.Map<IEnumerable<ChatHistory>>(messages);
    }

    public async Task<ChatHistorySummary> GetChatHistorySummaryAsync(Guid? agentId = null)
    {
        var messages = await chatRepository.GetHistoryAsync(agentId);
        var messageList = messages.ToList();

        var summary = new ChatHistorySummary
        {
            TotalMessages = messageList.Count,
            TotalTokens = messageList.Sum(ch => ch.TokenCount ?? 0),
            OldestMessage = messageList.Min(ch => ch.Timestamp),
            NewestMessage = messageList.Max(ch => ch.Timestamp),
            MessagesByAgent = messageList
                .GroupBy(ch => ch.AgentName)
                .Select(g => new { AgentName = g.Key, Count = g.Count() })
                .ToDictionary(x => x.AgentName, x => x.Count),
            MessagesByRole = messageList
                .GroupBy(ch => ch.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Role, x => x.Count),
            MessagesByDate = messageList
                .GroupBy(ch => ch.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Date, x => x.Count)
        };

        return summary;
    }

    // Chat Sessions Management
    public async Task<IEnumerable<ChatSession>> GetChatSessionsAsync(Guid? agentId = null)
    {
        var messages = await chatRepository.GetHistoryAsync(agentId);
        var messageList = messages.ToList();

        var sessions = messageList
            .Where(ch => !string.IsNullOrEmpty(ch.SessionId))
            .GroupBy(ch => ch.SessionId)
            .Select(g => new ChatSession
            {
                SessionId = g.Key,
                AgentId = g.First().AgentId ?? Guid.Empty,
                AgentName = g.First().AgentName ?? string.Empty,
                StartedAt = g.Min(ch => ch.Timestamp),
                LastActivityAt = g.Max(ch => ch.Timestamp),
                MessageCount = g.Count(),
                TotalTokens = g.Sum(ch => ch.TokenCount ?? 0),
                IsActive = g.Max(ch => ch.Timestamp) >
                           DateTime.UtcNow.AddHours(-1), // Consider active if last message was within 1 hour
                Description = g.Where(ch => ch.Role == "user").OrderBy(ch => ch.Timestamp).FirstOrDefault()?.Content
            })
            .OrderByDescending(s => s.LastActivityAt)
            .ToList();

        return sessions;
    }

    public async Task<ChatSession> GetChatSessionAsync(string sessionId)
    {
        var messages = await chatRepository.GetHistoryBySessionId(sessionId);
        var messageList = messages.ToList();

        var session = messageList
            .GroupBy(ch => ch.SessionId)
            .Select(g => new ChatSession
            {
                SessionId = g.Key,
                AgentId = g.First().AgentId ?? Guid.Empty,
                AgentName = g.First().AgentName ?? string.Empty,
                StartedAt = g.Min(ch => ch.Timestamp),
                LastActivityAt = g.Max(ch => ch.Timestamp),
                MessageCount = g.Count(),
                TotalTokens = g.Sum(ch => ch.TokenCount ?? 0),
                IsActive = g.Max(ch => ch.Timestamp) > DateTime.UtcNow.AddHours(-1),
                Description = g.Where(ch => ch.Role == "user").OrderBy(ch => ch.Timestamp).FirstOrDefault()?.Content
            })
            .FirstOrDefault();

        return session ?? new ChatSession();
    }

    public async Task<ChatSessionSummary> GetChatSessionSummaryAsync(string sessionId)
    {
        var session = await GetChatSessionAsync(sessionId);
        if (string.IsNullOrEmpty(session.SessionId))
        {
            return new ChatSessionSummary();
        }

        var messages = await chatRepository.GetHistoryBySessionId(sessionId);
        var recentMessages = messages
            .OrderByDescending(ch => ch.Timestamp)
            .Take(5)
            .Select(ch => new ChatMessagePreview
            {
                Id = ch.Id,
                Content = ch.Content.Length > 100 ? ch.Content.Substring(0, 100) + "..." : ch.Content,
                Role = ch.Role,
                Timestamp = ch.Timestamp,
                IsToolExecution = ch.IsToolExecution,
                ToolName = ch.ToolName
            })
            .ToList();

        return new ChatSessionSummary
        {
            SessionId = session.SessionId,
            AgentId = session.AgentId,
            AgentName = session.AgentName,
            StartedAt = session.StartedAt,
            LastActivityAt = session.LastActivityAt,
            MessageCount = session.MessageCount,
            TotalTokens = session.TotalTokens,
            SessionTitle = session.SessionTitle,
            Description = session.Description,
            RecentMessages = recentMessages,
            IsActive = session.IsActive
        };
    }

    public async Task<string> CreateNewChatSessionAsync(Guid agentId, string? sessionTitle = null) =>
        await chatRepository.CreateNewSessionAsync(agentId, sessionTitle);

    public async Task<bool> CloseChatSessionAsync(string sessionId) =>
        await chatRepository.CloseSessionAsync(sessionId);

    public async Task<bool> DeleteChatSessionAsync(string sessionId) =>
        await chatRepository.DeleteSessionAsync(sessionId);

    public async Task<bool> RenameChatSessionAsync(string sessionId, string newTitle) =>
        await chatRepository.RenameSessionAsync(sessionId, newTitle);

    // Chat Message Management
    public async Task<ChatHistory> SaveChatMessageAsync(ChatMessage message)
    {
        var chatMessage = mapper.Map<ChatMessageEntity>(message);
        chatMessage.Id = Guid.NewGuid();
        chatMessage.Timestamp = DateTime.UtcNow;

        var savedMessage = await chatRepository.SaveMessageAsync(chatMessage);
        return mapper.Map<ChatHistory>(savedMessage);
    }

    public async Task<bool> DeleteChatMessageAsync(Guid messageId) =>
        await chatRepository.DeleteMessageAsync(messageId);

    public async Task<bool> UpdateChatMessageAsync(Guid messageId, string newContent) =>
        await chatRepository.UpdateMessageAsync(messageId, newContent);

    public async Task<IEnumerable<ChatHistory>> SearchChatMessagesAsync(string searchTerm, Guid? agentId = null)
    {
        var messages = await chatRepository.SearchMessagesAsync(searchTerm, agentId);
        return mapper.Map<IEnumerable<ChatHistory>>(messages);
    }

    // Analytics and Insights
    public async Task<Dictionary<string, int>> GetMessageCountByAgentAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await chatRepository.GetMessageCountByAgentAsync(fromDate, toDate);

    public async Task<Dictionary<string, int>> GetMessageCountByRoleAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await chatRepository.GetMessageCountByRoleAsync(fromDate, toDate);

    public async Task<Dictionary<DateTime, int>> GetMessageCountByDateAsync(DateTime? fromDate = null,
        DateTime? toDate = null) => await chatRepository.GetMessageCountByDateAsync(fromDate, toDate);

    public async Task<int> GetTotalTokenUsageAsync(DateTime? fromDate = null, DateTime? toDate = null) =>
        await chatRepository.GetTotalTokenUsageAsync(fromDate, toDate);

    // Cleanup and Maintenance
    public async Task<bool> CleanupOldChatHistoryAsync(int daysToKeep) =>
        await chatRepository.CleanupOldHistoryAsync(daysToKeep);

    public async Task<bool> ArchiveChatSessionAsync(string sessionId) =>
        await chatRepository.ArchiveSessionAsync(sessionId);

    public async Task<bool> ExportChatSessionAsync(string sessionId, string format = "json")
    {
        var messages = await GetChatHistoryBySessionAsync(sessionId);
        var session = await GetChatSessionAsync(sessionId);

        if (!messages.Any())
        {
            return false;
        }

        // For now, just return true - in a real implementation, you'd export to file
        // This could be implemented to export to JSON, CSV, PDF, etc.
        return true;
    }

    // Agent-specific database operations
    public async Task<Agent?> GetAgentWithConfigAsync(Guid agentId)
    {
        var agent = await agentRepository.GetWithConfigAsync(agentId);
        return agent != null ? mapper.Map<Agent>(agent) : null;
    }
}
