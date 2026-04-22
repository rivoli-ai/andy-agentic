using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing agents and their configurations.  
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController(IAgentService agentService, IMapper mapper, IAuthService authService) : ControllerBase
{
    /// <summary>
    ///     Retrieves all agents visible to the current user.
    ///     Returns public agents and agents created by the current user.
    /// </summary>
    /// <returns>A list of visible agents.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents()
    {
        try
        {
            var currentUser = await authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var agents = await agentService.GetVisibleAgentsAsync(currentUser.Id);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific agent by its identifier if visible to the current user.
    /// </summary>
    /// <param name="id">The unique identifier of the agent.</param>
    /// <returns>The agent details if found and visible.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(Guid id)
    {
        try
        {
            var currentUser = await authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var agent = await agentService.GetVisibleAgentByIdAsync(id, currentUser.Id);
            if (agent == null)
            {
                return NotFound(new { error = "Agent not found or not accessible" });
            }

            return Ok(agent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Creates a new agent with the provided configuration.
    /// </summary>
    /// <param name="createAgentDto">The agent data for creation.</param>
    /// <returns>The created agent details.</returns>
    [HttpPost]
     [Authorize(Policy = "WriteRole")]
    public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] AgentDto createAgentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current user and set the CreatedByUserId
            var currentUser = await authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var agent = mapper.Map<Agent>(createAgentDto);
            agent.CreatedByUserId = currentUser.Id;
            ApplyLlmConfigIdsFromDto(agent, createAgentDto);

            var createdAgent = await agentService.CreateAgentAsync(agent);
            return CreatedAtAction(nameof(GetAgent), new { id = createdAgent.Id }, createdAgent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Updates an existing agent with new configuration data.
    ///     Only the owner of the agent can update it.
    /// </summary>
    /// <param name="id">The ID of the agent to update.</param>
    /// <param name="updateAgentDto">The updated agent data.</param>
    /// <returns>The updated agent details.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "WriteRole")]
    public async Task<ActionResult<AgentDto>> UpdateAgent(Guid id, [FromBody] AgentDto updateAgentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            // Get the existing agent to check ownership and preserve data
            var existingAgent = await agentService.GetAgentByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound(new { error = "Agent not found" });
            }

            // Check if the current user owns this agent
            if (existingAgent.CreatedByUserId != currentUser.Id)
            {
                return Forbid("You can only update agents you created");
            }

            var agent = mapper.Map<Agent>(updateAgentDto);
            agent.Id = id; // Ensure the ID is set
            agent.CreatedByUserId = existingAgent.CreatedByUserId; // Preserve the original creator
            agent.CreatedAt = existingAgent.CreatedAt; // Preserve the original creation date
            agent.UpdatedAt = DateTime.UtcNow; // Update the modification timestamp

            ApplyLlmConfigIdsFromDto(agent, updateAgentDto);

            // Clients often omit top-level llmConfigId; nested llmConfig.id or prior row is used next.
            if (agent.LlmConfigId == Guid.Empty)
                agent.LlmConfigId = existingAgent.LlmConfigId;

            // Do not reset execution metrics when the DTO carries defaults.
            agent.ExecutionCount = existingAgent.ExecutionCount;

            if (agent.LlmConfigId == Guid.Empty)
                return BadRequest(new { error = "LLM configuration is required. Set llmConfigId or llmConfig.id." });
            
            var updatedAgent = await agentService.UpdateAgentAsync(agent);
            return Ok(updatedAgent);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Deletes an agent by its identifier.
    ///     Only the owner of the agent can delete it.
    /// </summary>
    /// <param name="id">The unique identifier of the agent to delete.</param>
    /// <returns>Success or error response.</returns>
    [HttpDelete("{id}")]
     [Authorize(Policy = "WriteRole")]
    public async Task<ActionResult> DeleteAgent(Guid id)
    {
        try
        {
            var currentUser = await authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            // Check if the agent exists and if the current user owns it
            var existingAgent = await agentService.GetAgentByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound(new { error = "Agent not found" });
            }

            if (existingAgent.CreatedByUserId != currentUser.Id)
            {
                return Forbid("You can only delete agents you created");
            }

            var success = await agentService.DeleteAgentAsync(id);
            if (!success)
            {
                return NotFound(new { error = "Agent not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Searches for agents using a free-text query.
    /// </summary>
    /// <param name="q">The search query string.</param>
    /// <returns>A list of agents matching the search criteria.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<AgentDto>>> SearchAgents([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            var agents = await agentService.SearchAgentsAsync(q);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves agents filtered by their type.
    /// </summary>
    /// <param name="type">The agent type to filter by.</param>
    /// <returns>A list of agents of the specified type.</returns>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgentsByType(string type)
    {
        try
        {
            var agents = await agentService.GetAgentsByTypeAsync(type);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves agents filtered by a specific tag.
    /// </summary>
    /// <param name="tag">The tag to filter agents by.</param>
    /// <returns>A list of agents associated with the specified tag.</returns>
    [HttpGet("tag/{tag}")]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgentsByTag(string tag)
    {
        try
        {
            var agents = await agentService.GetAgentsByTagAsync(tag);
            return Ok(agents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// UIs often send only <c>llmConfig.id</c> / <c>embeddingLlmConfig.id</c> while leaving flat FK properties default.
    /// </summary>
    private static void ApplyLlmConfigIdsFromDto(Agent agent, AgentDto dto)
    {
        var chatId = dto.LlmConfig?.Id is Guid dtoChat && dtoChat != Guid.Empty
            ? dtoChat
            : agent.LlmConfig is { Id: var lid } && lid != Guid.Empty
                ? lid
                : (Guid?)null;
        if (agent.LlmConfigId == Guid.Empty && chatId is { } cid)
            agent.LlmConfigId = cid;

        var embedId = dto.EmbeddingLlmConfig?.Id is Guid dtoEmb && dtoEmb != Guid.Empty
            ? dtoEmb
            : agent.EmbeddingLlmConfig is { Id: var eid } && eid != Guid.Empty
                ? eid
                : (Guid?)null;
        if ((!agent.EmbeddingLlmConfigId.HasValue || agent.EmbeddingLlmConfigId == Guid.Empty) &&
            embedId is { } e)
            agent.EmbeddingLlmConfigId = e;
    }
}
