using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing skill registry connections and proxying registry search.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillRegistriesController(ISkillManager skillManager, IMapper mapper, ILogger<SkillRegistriesController> logger) : ControllerBase
{
    /// <summary>Lists all configured skill registry connections (credentials redacted).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillRegistryDto>>> GetRegistries()
    {
        try
        {
            var registries = await skillManager.GetRegistriesAsync();
            return Ok(registries.Select(ToRedactedDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Gets a single registry connection by id (credentials redacted).</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SkillRegistryDto>> GetRegistry(Guid id)
    {
        try
        {
            var registry = await skillManager.GetRegistryAsync(id);
            return registry == null
                ? NotFound(new { error = "Skill registry not found" })
                : Ok(ToRedactedDto(registry));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Creates a new registry connection.</summary>
    [HttpPost]
    [Authorize(Policy = "WriteRole")]
    public async Task<ActionResult<SkillRegistryDto>> CreateRegistry([FromBody] SkillRegistryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await skillManager.CreateRegistryAsync(mapper.Map<SkillRegistry>(dto));
            return CreatedAtAction(nameof(GetRegistry), new { id = created.Id }, ToRedactedDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create skill registry");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Updates an existing registry connection.</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "WriteRole")]
    public async Task<ActionResult<SkillRegistryDto>> UpdateRegistry(Guid id, [FromBody] SkillRegistryDto dto)
    {
        try
        {
            dto.Id = id;
            var updated = await skillManager.UpdateRegistryAsync(mapper.Map<SkillRegistry>(dto));
            return Ok(ToRedactedDto(updated));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update skill registry {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Deletes a registry connection.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "WriteRole")]
    public async Task<IActionResult> DeleteRegistry(Guid id)
    {
        try
        {
            var deleted = await skillManager.DeleteRegistryAsync(id);
            return deleted ? NoContent() : NotFound(new { error = "Skill registry not found" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Tests connectivity to a registry connection.</summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestRegistry(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await skillManager.TestRegistryAsync(id, cancellationToken);
            return Ok(new { success = ok });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>Searches a registry connection for skills (proxied so credentials stay server-side).</summary>
    [HttpGet("{id}/search")]
    public async Task<ActionResult<IEnumerable<SkillSearchResultDto>>> Search(Guid id, [FromQuery] string? q, CancellationToken cancellationToken)
    {
        try
        {
            var results = await skillManager.SearchAsync(id, q ?? string.Empty, cancellationToken);
            return Ok(mapper.Map<List<SkillSearchResultDto>>(results));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Skill registry search failed for {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    private SkillRegistryDto ToRedactedDto(SkillRegistry registry)
    {
        var dto = mapper.Map<SkillRegistryDto>(registry);
        dto.AuthConfig = null; // never return stored credentials
        return dto;
    }
}
