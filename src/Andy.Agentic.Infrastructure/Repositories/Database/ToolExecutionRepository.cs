using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository implementation for managing ToolExecutionLog entities in the database.
    /// Provides functionality for logging tool executions, retrieving execution logs by various criteria,
    /// and tracking tool usage patterns. Supports filtering by agent ID, session ID, and time-based queries
    /// for monitoring and debugging tool execution workflows.
    /// </summary>
    public class ToolExecutionRepository(AndyDbContext context) : IToolExecutionRepository
    {
        public async Task<ToolExecutionLogEntity> LogExecutionAsync(ToolExecutionLogEntity log)
        {
            context.ToolExecutionLogs.Add(log);
            await context.SaveChangesAsync();
            return log;
        }

        public async Task<ToolExecutionLogEntity?> GetLogByIdAsync(Guid executionId)
        {
            return await context.ToolExecutionLogs.FindAsync(executionId);
        }

        public async Task<IEnumerable<ToolExecutionLogEntity>> GetRecentExecutionsAsync(Guid? agentId, string sessionId)
        {
            var query = context.ToolExecutionLogs.AsQueryable();

            if (agentId.HasValue)
                query = query.Where(log => log.AgentId == agentId);

            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(log => log.SessionId == sessionId);

            var logs = await query
                .OrderByDescending(log => log.ExecutedAt)
                .ToListAsync();

            return logs;
        }

        public async Task<IEnumerable<ToolExecutionLogEntity>> GetRecentExecutionsAsync(int count)
        {
            var logs = await context.ToolExecutionLogs
                .OrderByDescending(log => log.ExecutedAt)
                .Take(count)
                .ToListAsync();

            return logs;
        }
    }
}

