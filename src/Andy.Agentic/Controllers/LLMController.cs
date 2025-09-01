using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing Large Language Model (LLM) configurations and providers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LlmController(ILlmService llmService, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Retrieves all LLM configurations.
    /// </summary>
    /// <returns>A list of all LLM configurations.</returns>
    [HttpGet("configs")]
    public async Task<ActionResult<IEnumerable<LlmConfigDto>>> GetLlmConfigs()
    {
        try
        {
            var configs = await llmService.GetAllLlmConfigsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific LLM configuration by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration.</param>
    /// <returns>The LLM configuration details if found.</returns>
    [HttpGet("configs/{id}")]
    public async Task<ActionResult<LlmConfigDto>> GetLlmConfig(Guid id)
    {
        try
        {
            var config = await llmService.GetLlmConfigByIdAsync(id);
            if (config == null)
            {
                return NotFound(new { error = "LLM Config not found" });
            }

            return Ok(config);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Creates a new LLM configuration.
    /// </summary>
    /// <param name="createLlmConfigDto">The LLM configuration data for creation.</param>
    /// <returns>The created LLM configuration details.</returns>
    [HttpPost("configs")]
    public async Task<ActionResult<LlmConfigDto>> CreateLlmConfig([FromBody] LlmConfigDto createLlmConfigDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var config = await llmService.CreateLlmConfigAsync(mapper.Map<LlmConfig>(createLlmConfigDto));
            return CreatedAtAction(nameof(GetLlmConfig), new { id = config.Id }, config);
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
    ///     Updates an existing LLM configuration.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration to update.</param>
    /// <param name="updateLlmConfigDto">The updated LLM configuration data.</param>
    /// <returns>The updated LLM configuration details.</returns>
    [HttpPut("configs/{id}")]
    public async Task<ActionResult<LlmConfigDto>> UpdateLlmConfig([FromBody] LlmConfigDto updateLlmConfigDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var config = await llmService.UpdateLlmConfigAsync(mapper.Map<LlmConfig>(updateLlmConfigDto));
            return Ok(config);
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
    ///     Deletes an LLM configuration by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM configuration to delete.</param>
    /// <returns>Success or error response.</returns>
    [HttpDelete("configs/{id}")]
    public async Task<ActionResult> DeleteLlmConfig(Guid id)
    {
        try
        {
            var success = await llmService.DeleteLlmConfigAsync(id);
            if (!success)
            {
                return NotFound(new { error = "LLM Config not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves all available LLM providers.
    /// </summary>
    /// <returns>A list of available LLM providers.</returns>
    [HttpGet("providers")]
    public ActionResult<IEnumerable<LlmProviderDto>> GetProviders()
    {
        try
        {
            var providers = llmService.GetProvidersAsync();
            return Ok(providers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific LLM provider by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the LLM provider.</param>
    /// <returns>The LLM provider details if found.</returns>
    [HttpGet("providers/{id}")]
    public ActionResult<LlmProviderDto> GetProvider(string id)
    {
        try
        {
            var provider = llmService.GetProviderByIdAsync(id);
            if (provider == null)
            {
                return NotFound(new { error = "Provider not found" });
            }

            return Ok(provider);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Tests the connectivity to an LLM provider using the provided configuration.
    /// </summary>
    /// <param name="testConnectionDto">The connection test parameters including provider details.</param>
    /// <returns>The test result indicating success or failure with details.</returns>
    [HttpPost("test-connection")]
    public async Task<ActionResult<TestConnectionResultDto>> TestConnection(
        [FromBody] TestConnectionDto testConnectionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await llmService.TestConnectionAsync(mapper.Map<TestConnection>(testConnectionDto));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
