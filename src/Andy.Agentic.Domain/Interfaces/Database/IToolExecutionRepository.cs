using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides operations for logging and querying tool executions.
/// </summary>
public interface IToolExecutionRepository
{
    /// <summary>
    ///     Persists a tool execution log entry.
    /// </summary>
    Task<ToolExecutionLogEntity> LogExecutionAsync(ToolExecutionLogEntity log);

    /// <summary>
    ///     Retrieves a log entry by execution identifier.
    /// </summary>
    Task<ToolExecutionLogEntity?> GetLogByIdAsync(Guid executionId);

    /// <summary>
    ///     Gets recent executions optionally filtered by agent and session.
    /// </summary>
    Task<IEnumerable<ToolExecutionLogEntity>> GetRecentExecutionsAsync(Guid? agentId, string sessionId);

    /// <summary>
    ///     Gets a fixed number of recent executions.
    /// </summary>
    Task<IEnumerable<ToolExecutionLogEntity>> GetRecentExecutionsAsync(int count);
}
