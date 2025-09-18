using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing tools available to agents.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolsController(IToolService toolService, IMapper mapper, IMcpService mcpService) : ControllerBase
{
    /// <summary>
    ///     Retrieves all available tools.
    /// </summary>
    /// <returns>A list of all tools in the system.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ToolDto>>> GetTools()
    {
        try
        {
            var tools = await toolService.GetAllToolsAsync();
            return Ok(tools);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific tool by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tool.</param>
    /// <returns>The tool details if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ToolDto>> GetTool(Guid id)
    {
        try
        {
            var tool = await toolService.GetToolByIdAsync(id);
            if (tool == null)
            {
                return NotFound(new { error = "Tool not found" });
            }

            return Ok(tool);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Creates a new tool with the provided configuration.
    /// </summary>
    /// <param name="createToolDto">The tool data for creation.</param>
    /// <returns>The created tool details.</returns>
    [HttpPost]
    public async Task<ActionResult<ToolDto>> CreateTool([FromBody] ToolDto createToolDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tool = await toolService.CreateToolAsync(mapper.Map<Tool>(createToolDto));
            return CreatedAtAction(nameof(GetTool), new { id = tool.Id }, tool);
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
    ///     Updates an existing tool with new configuration.
    /// </summary>
    /// <param name="id">The unique identifier of the tool to update.</param>
    /// <param name="updateToolDto">The updated tool data.</param>
    /// <returns>The updated tool details.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ToolDto>> UpdateTool(Guid id, [FromBody] ToolDto updateToolDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tool = await toolService.UpdateToolAsync(id, mapper.Map<Tool>(updateToolDto));
            return Ok(tool);
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
    ///     Deletes a tool by its identifier if it's not being used by any agents.
    /// </summary>
    /// <param name="id">The unique identifier of the tool to delete.</param>
    /// <returns>Success or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTool(Guid id)
    {
        try
        {
            var success = await toolService.DeleteToolAsync(id);
            if (!success)
            {
                return NotFound(new { error = "Tool not found" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Cannot delete tool", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Searches for tools using a free-text query.
    /// </summary>
    /// <param name="q">The search query string.</param>
    /// <returns>A list of tools matching the search criteria.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ToolDto>>> SearchTools([FromQuery] string q)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            var tools = await toolService.SearchToolsAsync(q);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves tools filtered by category.
    /// </summary>
    /// <param name="category">The category to filter tools by.</param>
    /// <returns>A list of tools in the specified category.</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<ToolDto>>> GetToolsByCategory(string category)
    {
        try
        {
            var tools = await toolService.GetToolsByCategoryAsync(category);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves tools filtered by type.
    /// </summary>
    /// <param name="type">The type to filter tools by.</param>
    /// <returns>A list of tools of the specified type.</returns>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<ToolDto>>> GetToolsByType(string type)
    {
        try
        {
            var tools = await toolService.GetToolsByTypeAsync(type);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves only active tools that are available for use.
    /// </summary>
    /// <returns>A list of active tools.</returns>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ToolDto>>> GetActiveTools()
    {
        try
        {
            var tools = await toolService.GetActiveToolsAsync();
            return Ok(tools);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Discovers tools from an MCP server at the specified URL.
    /// </summary>
    /// <param name="url">The URL of the MCP server.</param>
    /// <returns>A list of discovered tools from the MCP server.</returns>
    [HttpGet("discover-mcp")]
    public async Task<ActionResult<McpToolDiscoveryResponse>> DiscoverMcpTools([FromQuery] string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new { error = "URL is required" });
            }

            var response = await mcpService.DiscoverToolsAsync(url);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Discovers tools from an MCP server and returns them as Tool entities.
    /// </summary>
    /// <param name="request">The MCP discovery request containing the server URL.</param>
    /// <returns>A list of Tool entities discovered from the MCP server.</returns>
    [HttpPost("discover-mcp-tools")]
    public async Task<ActionResult<IEnumerable<ToolDto>>> DiscoverMcpToolsAsEntities([FromBody] McpDiscoveryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { error = "URL is required" });
            }

            var response = await mcpService.DiscoverToolsAsync(request.Url);
            
            if (!response.Success)
            {
                return BadRequest(new { error = "MCP discovery failed", message = response.Error });
            }

            var tools = response.Tools.Select(mcpTool => mcpService.ConvertToTool(mcpTool, request.Url));
            var toolDtos = tools.Select(tool => mapper.Map<ToolDto>(tool));

            return Ok(toolDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
