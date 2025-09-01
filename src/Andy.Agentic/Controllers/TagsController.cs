using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Andy.Agentic.Controllers;

/// <summary>
///     API controller for managing tags used to categorize and organize agents.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ITagService _tagService;

    /// <summary>
    ///     Initializes a new instance of the TagsController.
    /// </summary>
    /// <param name="tagService">The tag service for business logic operations.</param>
    /// <param name="mapper"></param>
    public TagsController(ITagService tagService,
        IMapper mapper)
    {
        _tagService = tagService;
        _mapper = mapper;
    }

    /// <summary>
    ///     Retrieves all available tags.
    /// </summary>
    /// <returns>A list of all tags in the system.</returns>
    // GET: api/tags
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        try
        {
            var tags = await _tagService.GetTagsAsync();
            return Ok(_mapper.Map<IEnumerable<TagDto>>(tags));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Retrieves a specific tag by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag.</param>
    /// <returns>The tag details if found.</returns>
    // GET: api/tags/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(Guid id)
    {
        try
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            return Ok(_mapper.Map<TagDto>(tag));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new tag with the provided information.
    /// </summary>
    /// <param name="createTagDto">The tag data for creation.</param>
    /// <returns>The created tag details.</returns>
    // POST: api/tags
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] TagDto createTagDto)
    {
        try
        {
            var tag = await _tagService.CreateTagAsync(_mapper.Map<Tag>(createTagDto));
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Updates an existing tag with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to update.</param>
    /// <param name="updateTagDto">The updated tag data.</param>
    /// <returns>Success or error response.</returns>
    // PUT: api/tags/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] TagDto updateTagDto)
    {
        try
        {
            var tag = await _tagService.UpdateTagAsync(id, _mapper.Map<Tag>(updateTagDto));
            return Ok(tag);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }

            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Deletes a tag by its identifier if it's not being used by any agents.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete.</param>
    /// <returns>Success or error response.</returns>
    // DELETE: api/tags/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        try
        {
            var deleted = await _tagService.DeleteTagAsync(id);
            if (!deleted)
            {
                return NotFound($"Tag with ID {id} not found");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Searches for tags using a text query against name and description.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of tags matching the search criteria.</returns>
    // GET: api/tags/search?query={searchTerm}
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TagDto>>> SearchTags([FromQuery] string query)
    {
        try
        {
            var tags = await _tagService.SearchTagsAsync(query);
            return Ok(_mapper.Map<IEnumerable<TagDto>>(tags));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
