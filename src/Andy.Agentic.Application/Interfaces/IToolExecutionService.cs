using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

public interface IToolExecutionService
{
    Task<ToolExecutionLog> ExecuteToolAsync(ToolExecutionLog request);
    Task<IEnumerable<ToolExecutionLog>> GetExecutionLogsAsync(Guid? agentId = null, string? sessionId = null);
    Task<ToolExecutionLog?> GetExecutionLogByIdAsync(Guid executionId);
    Task<IEnumerable<ToolExecutionLog>> GetRecentExecutionsAsync(int count = 10);
    Task<List<ToolExecutionLog>> ExecuteToolCallsAsync(List<ToolCall> toolCalls, Agent agent, string sessionId);
    string CreateFollowUpMessage(List<string> toolResults);
}
