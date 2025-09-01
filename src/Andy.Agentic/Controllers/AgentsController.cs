using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing agents and their configurations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AgentsController(IAgentService agentService, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Retrieves all agents.
    /// </summary>
    /// <returns>A list of all agents.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents()
    {
        try
        {
            var agents = await agentService.GetAllAgentsAsync();
            return Ok(agents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific agent by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the agent.</param>
    /// <returns>The agent details if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetAgent(Guid id)
    {
        try
        {
            var agent = await agentService.GetAgentByIdAsync(id);
            if (agent == null)
            {
                return NotFound(new { error = "Agent not found" });
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
    public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] AgentDto createAgentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var agent = await agentService.CreateAgentAsync(mapper.Map<Agent>(createAgentDto));
            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agent);
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
    /// </summary>
    /// <param name="updateAgentDto">The updated agent data.</param>
    /// <returns>The updated agent details.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<AgentDto>> UpdateAgent([FromBody] AgentDto updateAgentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var agent = await agentService.UpdateAgentAsync(mapper.Map<Agent>(updateAgentDto));
            return Ok(agent);
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
    /// </summary>
    /// <param name="id">The unique identifier of the agent to delete.</param>
    /// <returns>Success or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAgent(Guid id)
    {
        try
        {
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
}
