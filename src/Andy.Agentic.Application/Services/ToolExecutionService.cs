using System.Diagnostics;
using System.Text.Json;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service for executing tools, managing tool calls, and logging tool execution results.
///     Handles tool preparation, execution, and result processing for agent interactions.
/// </summary>
public class ToolExecutionService(
    IDataBaseService databaseResourceAccess,
    IToolService toolEngine,
    IEnumerable<IToolProvider> toolProviders)
    : IToolExecutionService
{
    /// <summary>
    ///     Executes multiple tool calls for an agent and returns the results as formatted messages.
    /// </summary>
    /// <param name="toolCalls">The list of tool calls to execute.</param>
    /// <param name="agent">The agent configuration containing available tools.</param>
    /// <param name="sessionId">The chat session identifier for logging.</param>
    /// <returns>A list of formatted result messages from tool executions.</returns>
    public async Task<List<ToolExecutionLog>> ExecuteToolCallsAsync(
        List<ToolCall> toolCalls,
        Agent agent,
        string sessionId)
    {
        var toolResults = new List<ToolExecutionLog>();
        var preparedExecutions = PrepareToolExecutions(toolCalls, agent);

        foreach (var (request, error) in preparedExecutions)
        {
            if (error != null)
            {
                toolResults.Add(error);
                continue;
            }

            try
            {
                request.SessionId = sessionId;

                var toolResult = await ExecuteToolAsync(request);

                toolResults.Add(toolResult);
            }
            catch (Exception ex)
            {
                toolResults.Add(new ToolExecutionLog{ErrorMessage = ex.Message,Success = false});
            }
        }

        return toolResults;
    }

    /// <summary>
    ///     Creates a follow-up message combining the original message with tool execution results.
    /// </summary>
    /// <param name="originalMessage">The original message to include in the follow-up.</param>
    /// <param name="toolResults">The list of tool execution result messages.</param>
    /// <returns>A formatted message combining the original content with tool results.</returns>
    public string CreateFollowUpMessage(List<string> toolResults)
    {
        var toolResultsSummary = string.Join("\n", toolResults);
        return $"Tool execution completed. Results:\n{toolResultsSummary}.";
    }

    /// <summary>
    ///     Executes a single tool based on the provided execution request.
    /// </summary>
    /// <param name="request">The tool execution request containing tool ID and parameters.</param>
    /// <returns>A tool execution log  with results and execution details.</returns>
    public async Task<ToolExecutionLog> ExecuteToolAsync(ToolExecutionLog request)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var tool = await toolEngine.GetToolByIdAsync(request.ToolId);
            if(tool == null)
            {
                throw new KeyNotFoundException($"the tool {request.ToolId} not found");
            }

           
            var provider = toolProviders.FirstOrDefault(p => p.CanHandleToolType(tool.Type));
            if (provider == null)
            {
                throw new NotSupportedException($"No provider found for tool type: {tool.Type}");
            }

            var result = await provider.ExecuteToolAsync(tool, request.Parameters);

            stopwatch.Stop();

            return await LogExecutionAsync(request, tool, result, true, null, stopwatch.ElapsedMilliseconds);

        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogExecutionAsync(request, null, null, false, ex.Message, stopwatch.ElapsedMilliseconds);

            return new ToolExecutionLog
            {
                Success = false,
                ErrorMessage = ex.Message,
                ToolName = request.ToolName,
                Parameters = request.Parameters,
                ExecutionTime = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<IEnumerable<ToolExecutionLog>> GetExecutionLogsAsync(Guid? agentId = null,
        string? sessionId = null) =>
        await databaseResourceAccess.GetRecentToolExecutionsAsync(agentId, sessionId);

    /// <summary>
    ///     Retrieves a specific tool execution log by its identifier.
    /// </summary>
    /// <param name="executionId">The unique identifier of the execution log.</param>
    /// <returns>The tool execution log  if found; otherwise, null.</returns>
    public async Task<ToolExecutionLog?> GetExecutionLogByIdAsync(Guid executionId)
    {
        var log = await databaseResourceAccess.GetToolExecutionLogByIdAsync(executionId);
        return log;
    }

    /// <summary>
    ///     Retrieves the most recent tool executions up to a specified count.
    /// </summary>
    /// <param name="count">The maximum number of recent executions to retrieve.</param>
    /// <returns>A collection of the most recent tool execution log s.</returns>
    public async Task<IEnumerable<ToolExecutionLog>> GetRecentExecutionsAsync(int count) =>
        await databaseResourceAccess.GetRecentToolExecutionsAsync(count);

    /// <summary>
    ///     Creates a follow-up message combining the original LLM message with tool execution results.
    ///     This method is not yet implemented.
    /// </summary>
    /// <param name="llmMessage">The original message from the LLM.</param>
    /// <param name="toolResults">The results from tool executions.</param>
    /// <returns>A combined message with tool results.</returns>
    /// <exception cref="NotImplementedException">This method is not yet implemented.</exception>
    public string CreateFollowUpMessage(string llmMessage, object toolResults) => throw new NotImplementedException();

    /// <summary>
    ///     Finds a tool by name within an agent's available tools.
    /// </summary>
    /// <param name="agent">The agent containing tool configurations.</param>
    /// <param name="toolName">The name of the tool to find.</param>
    /// <returns>The tool  if found; otherwise, null.</returns>
    public Tool? FinolByName(Agent agent, string toolName) =>
        agent.Tools
            .FirstOrDefault(at => at.Tool.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase))?.Tool;

    /// <summary>
    ///     Parses tool call arguments from a JSON string into a dictionary.
    /// </summary>
    /// <param name="arguments">The JSON string containing tool arguments.</param>
    /// <returns>A dictionary of argument names and values; empty if parsing fails.</returns>
    public Dictionary<string, object> ParseToolCallArguments(string arguments)
    {
        try
        {
            if (string.IsNullOrEmpty(arguments))
            {
                return new Dictionary<string, object>();
            }

            var parsedParams = JsonSerializer.Deserialize<Dictionary<string, object>>(arguments);
            return parsedParams ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    ///     Creates an error response  for tool execution failures.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A tool execution log  with error information.</returns>
    public ToolExecutionLog CreateErrorResponse(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

    /// <summary>
    ///     Prepares a single tool execution by validating the tool and parsing arguments.
    /// </summary>
    /// <param name="toolCall">The tool call to prepare for execution.</param>
    /// <param name="agent">The agent configuration containing available tools.</param>
    /// <returns>A tuple containing the prepared execution request or an error response.</returns>
    public (ToolExecutionLog? Request, ToolExecutionLog? Error) PrepareToolExecution(
        ToolCall toolCall,
        Agent agent)
    {
        try
        {
            var tool = FinolByName(agent, toolCall.Function.Name);
            if (tool == null)
            {
                return (null,
                    CreateErrorResponse($"Tool '{toolCall.Function.Name}' not found or not assigned to this agent"));
            }

            var parameters = ParseToolCallArguments(toolCall.Function.Arguments);

            var request = new ToolExecutionLog
            {
                ToolId = tool.Id,
                ToolName = toolCall.Function.Name,
                Parameters = parameters,
                SessionId = null, // Will be set by the caller
                AgentId = agent.Id
            };

            return (request, null);
        }
        catch (Exception ex)
        {
            return (null, CreateErrorResponse($"Error during tool preparation: {ex.Message}"));
        }
    }

    /// <summary>
    ///     Prepares multiple tool executions by validating tools and parsing arguments.
    /// </summary>
    /// <param name="toolCalls">The collection of tool calls to prepare.</param>
    /// <param name="agent">The agent configuration containing available tools.</param>
    /// <returns>A list of tuples containing prepared execution requests or error responses.</returns>
    public List<(ToolExecutionLog Request, ToolExecutionLog? Error)> PrepareToolExecutions(
        IEnumerable<ToolCall> toolCalls,
        Agent agent)
    {
        var results = new List<(ToolExecutionLog Request, ToolExecutionLog? Error)>();

        foreach (var toolCall in toolCalls)
        {
            var result = PrepareToolExecution(toolCall, agent);
            if (result.Request != null)
            {
                results.Add((result.Request, null));
            }
            else if (result.Error != null)
            {
                // Create a placeholder request with error for consistency
                var placeholderRequest = new ToolExecutionLog
                {
                    ToolId = Guid.Empty,
                    ToolName = toolCall.Function.Name,
                    Parameters = new Dictionary<string, object>(),
                    SessionId = null,
                    AgentId = agent.Id
                };
                results.Add((placeholderRequest, result.Error));
            }
        }

        return results;
    }


    // Private helper methods

    /// <summary>
    ///     Logs a tool execution result to the database for audit and analysis purposes.
    /// </summary>
    /// <param name="request">The original execution request.</param>
    /// <param name="tool">The tool that was executed.</param>
    /// <param name="result">The result of the tool execution.</param>
    /// <param name="success">Whether the execution was successful.</param>
    /// <param name="errorMessage">Error message if execution failed.</param>
    /// <param name="executionTime">The execution time in milliseconds.</param>
    private async Task<ToolExecutionLog> LogExecutionAsync(
        ToolExecutionLog request,
        Tool? tool,
        object? result,
        bool success,
        string? errorMessage,
        long executionTime) =>
        await databaseResourceAccess.LogToolExecutionAsync(request, tool, result, success, errorMessage, executionTime);
}
