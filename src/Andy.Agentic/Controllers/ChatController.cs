using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing chat interactions and streaming conversations with agents.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController(IChatService chatService, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Sends a message to an agent and streams the response back in real-time using Server-Sent Events.
    /// </summary>
    /// <param name="chatMessage">The chat message containing content, agent ID, and session information.</param>
    // POST: api/chat/stream
    [HttpPost("stream")]
    public async Task SendMessageStream([FromBody] ChatMessageDto chatMessage)
    {
        // Set SSE headers
        Response.Headers["Content-Type"] = "text/event-stream; charset=utf-8";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["Access-Control-Allow-Origin"] = "*";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await foreach (var chunk in chatService.GetMessageStreamAsync(mapper.Map<ChatMessage>(chatMessage)))
            {
                var jsonResponse = JsonSerializer.Serialize(chunk,
                    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                await Response.WriteAsync($"data: {jsonResponse}\n\n", Encoding.UTF8);
                await Response.Body.FlushAsync();
                await Task.Delay(1);
            }
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new
            {
                error = new { message = ex.Message, type = "internal_error" }
            });
            await Response.WriteAsync($"data: {errorJson}\n\n", Encoding.UTF8);
            await Response.Body.FlushAsync();
        }
    }

    /// <summary>
    ///     Retrieves the chat history for a specific agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <returns>A list of chat history entries for the agent.</returns>
    // GET: api/chat/history/{agentId}
    [HttpGet("history/{agentId}")]
    public async Task<ActionResult<IEnumerable<ChatHistoryDto>>> GetChatHistory(Guid agentId)
    {
        try
        {
            var history = await chatService.GetChatHistoryAsync(agentId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves all chat sessions, optionally filtered by agent.
    /// </summary>
    /// <param name="agentId">Optional agent identifier to filter sessions.</param>
    /// <returns>A list of chat session summaries.</returns>
    // GET: api/chat/sessions
    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ChatSessionSummaryDto>>> GetChatSessions(
        [FromQuery] Guid? agentId = null)
    {
        try
        {
            var sessions = await chatService.GetChatSessionsAsync(agentId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves details of a specific chat session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>The chat session details.</returns>
    // GET: api/chat/sessions/{sessionId}
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<ChatSessionDto>> GetChatSession(string sessionId)
    {
        try
        {
            var session = await chatService.GetChatSessionAsync(sessionId);

            return Ok(session);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves chat history for a specific session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>A list of chat history entries for the session.</returns>
    // GET: api/chat/sessions/{sessionId}/history
    [HttpGet("sessions/{sessionId}/history")]
    public async Task<ActionResult<IEnumerable<ChatHistoryDto>>> GetChatHistoryBySession(string sessionId)
    {
        try
        {
            var history = await chatService.GetChatHistoryBySessionAsync(sessionId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Alternative endpoint to retrieve chat history for a specific session (for frontend compatibility).
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <returns>A list of chat history entries for the session.</returns>
    // GET: api/chat/history/session/{sessionId} (alternative endpoint for frontend compatibility)
    [HttpGet("history/session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<ChatHistoryDto>>> GetChatHistoryBySessionAlternative(string sessionId)
    {
        try
        {
            var history = await chatService.GetChatHistoryBySessionAsync(sessionId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Creates a new chat session for an agent.
    /// </summary>
    /// <param name="request">The session creation request containing agent ID and optional title.</param>
    /// <returns>The created chat session details.</returns>
    // POST: api/chat/sessions
    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSessionDto>> CreateChatSession([FromBody] ChatSessionDto request)
    {
        try
        {
            var sessionId = await chatService.CreateNewChatSessionAsync(request.AgentId, request.Title);
            var sessionDto = await chatService.GetChatSessionAsync(sessionId);
            return Ok(sessionDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }


    /// <summary>
    ///     Closes an active chat session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to close.</param>
    /// <returns>Success or error response.</returns>
    // PUT: api/chat/sessions/{sessionId}/close
    [HttpPut("sessions/{sessionId}/close")]
    public async Task<IActionResult> CloseChatSession(string sessionId)
    {
        try
        {
            await chatService.CloseChatSessionAsync(sessionId);
            return Ok(new { message = "Session closed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Deletes a chat session and all its associated messages.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to delete.</param>
    /// <returns>Success or error response.</returns>
    // DELETE: api/chat/sessions/{sessionId}
    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> DeleteChatSession(string sessionId)
    {
        try
        {
            await chatService.DeleteChatSessionAsync(sessionId);
            return Ok(new { message = "Session deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Renames an existing chat session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to rename.</param>
    /// <param name="request">The rename request containing the new title.</param>
    /// <returns>The updated chat session details.</returns>
    // PUT: api/chat/sessions/{sessionId}/rename
    [HttpPut("sessions/{sessionId}/rename")]
    public async Task<ActionResult<ChatSessionDto>> RenameChatSession(string sessionId,
        [FromBody] ChatSessionDto request)
    {
        try
        {
            var session = await chatService.RenameChatSessionAsync(sessionId, request.Title);

            return Ok(session);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Searches chat messages using a text query, optionally filtered by agent.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="agentId">Optional agent identifier to filter search results.</param>
    /// <returns>A list of chat message previews matching the search criteria.</returns>
    // GET: api/chat/search
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ChatMessagePreviewDto>>> SearchChatMessages([FromQuery] string query,
        [FromQuery] Guid? agentId = null)
    {
        try
        {
            var results = await chatService.SearchChatMessagesAsync(query, agentId);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a summary of chat history for a specific agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <returns>A summary of the agent's chat history including statistics.</returns>
    // GET: api/chat/summary/{agentId}
    [HttpGet("summary/{agentId}")]
    public async Task<ActionResult<ChatHistorySummaryDto>> GetChatHistorySummary(Guid agentId)
    {
        try
        {
            var summary = await chatService.GetChatHistorySummaryAsync(agentId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Test endpoint to retrieve detailed chat history information for debugging purposes.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session to test.</param>
    /// <returns>Detailed debugging information about the session and its history.</returns>
    // GET: api/chat/test-history/{sessionId} (for debugging)
    [HttpGet("test-history/{sessionId}")]
    public async Task<ActionResult<object>> TestChatHistory(string sessionId)
    {
        try
        {
            var chatHistory = (await chatService.GetChatHistoryBySessionAsync(sessionId)).ToList();
            var session = await chatService.GetChatSessionAsync(sessionId);


            return Ok(new
            {
                SessionId = sessionId,
                SessionInfo = session,
                ChatHistory = chatHistory,
                MessageCount = chatHistory.Count(),
                HasHistory = chatHistory.Any()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
