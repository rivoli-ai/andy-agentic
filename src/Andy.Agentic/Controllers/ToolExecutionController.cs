using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for executing tools and managing tool execution logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolExecutionController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IToolExecutionService _toolExecutionEngine;

    /// <summary>
    ///     Initializes a new instance of the ToolExecutionController.
    /// </summary>
    /// <param name="toolExecutionEngine">The tool execution service.</param>
    public ToolExecutionController(IToolExecutionService toolExecutionEngine, IMapper mapper)
    {
        _toolExecutionEngine = toolExecutionEngine;
        _mapper = mapper;
    }

    /// <summary>
    ///     Executes a tool with the provided parameters and logs the execution.
    /// </summary>
    /// <param name="request">The tool execution request containing tool information and parameters.</param>
    /// <returns>The execution log with results and status.</returns>
    [HttpPost("execute")]
    public async Task<ActionResult<ToolExecutionLogDto>> ExecuteTool([FromBody] ToolExecutionLogDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _toolExecutionEngine.ExecuteToolAsync(_mapper.Map<ToolExecutionLog>(request));
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Tool execution failed", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves tool execution logs, optionally filtered by agent and session.
    /// </summary>
    /// <param name="agentId">Optional agent identifier to filter logs.</param>
    /// <param name="sessionId">Optional session identifier to filter logs.</param>
    /// <returns>A list of tool execution logs matching the criteria.</returns>
    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<ToolExecutionLogDto>>> GetExecutionLogs(
        [FromQuery] Guid? agentId = null,
        [FromQuery] string? sessionId = null)
    {
        try
        {
            var logs = await _toolExecutionEngine.GetExecutionLogsAsync(agentId, sessionId);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve execution logs", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific tool execution log by its identifier.
    /// </summary>
    /// <param name="executionId">The unique identifier of the execution log.</param>
    /// <returns>The execution log details if found.</returns>
    [HttpGet("logs/{executionId}")]
    public async Task<ActionResult<ToolExecutionLogDto>> GetExecutionLog(Guid executionId)
    {
        try
        {
            var log = await _toolExecutionEngine.GetExecutionLogByIdAsync(executionId);
            if (log == null)
            {
                return NotFound(new { error = "Execution log not found" });
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve execution log", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves the most recent tool executions up to a specified count.
    /// </summary>
    /// <param name="count">The maximum number of recent executions to retrieve (default: 10).</param>
    /// <returns>A list of the most recent tool execution logs.</returns>
    [HttpGet("logs/recent")]
    public async Task<ActionResult<IEnumerable<ToolExecutionLogDto>>> GetRecentExecutions([FromQuery] int count = 10)
    {
        try
        {
            var logs = await _toolExecutionEngine.GetRecentExecutionsAsync(count);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve recent executions", message = ex.Message });
        }
    }
}
