using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Services;

/// <summary>
/// Service for managing tags and their business logic.
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the TagService.
    /// </summary>
    /// <param name="tagRepository">The tag repository for data access.</param>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    public TagService(ITagRepository tagRepository, IMapper mapper)
    {
        _tagRepository = tagRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all available tags.
    /// </summary>
    /// <returns>A list of all tags in the system.</returns>
    public async Task<IEnumerable<Tag>> GetTagsAsync()
    {
        var tags = await _tagRepository.GetAllTagsAsync();
        return _mapper.Map<IEnumerable<Tag>>(tags);
    }

    /// <summary>
    /// Retrieves a specific tag by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag.</param>
    /// <returns>The tag details if found, null otherwise.</returns>
    public async Task<Tag?> GetTagByIdAsync(Guid id)
    {
        var tag = await _tagRepository.GetTagByIdAsync(id);
        return _mapper.Map<Tag>(tag);
    }

    /// <summary>
    /// Retrieves a tag by name.
    /// </summary>
    /// <param name="name">The name of the tag to find.</param>
    /// <returns>The tag details if found, null otherwise.</returns>
    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        var tag = await _tagRepository.GetTagByNameAsync(name);
        return _mapper.Map<Tag>(tag);
    }

    /// <summary>
    /// Creates a new tag with the provided information.
    /// </summary>
    /// <param name="createTag">The tag data for creation.</param>
    /// <returns>The created tag details.</returns>
    public async Task<Tag> CreateTagAsync(Tag createTag)
    {
        if (string.IsNullOrWhiteSpace(createTag.Name))
        {
            throw new ArgumentException("Tag name is required", nameof(createTag));
        }

        // Check if tag with same name already exists
        var existingTag = await _tagRepository.GetTagByNameAsync(createTag.Name);
        if (existingTag != null)
        {
            throw new InvalidOperationException($"Tag with name '{createTag.Name}' already exists");
        }

        var tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = createTag.Name.Trim(),
            Description = createTag.Description?.Trim(),
            Color = createTag.Color ?? GenerateRandomColor(),
            CreatedAt = DateTime.UtcNow
        };

        var createdTag = await _tagRepository.CreateTagAsync(tag);
        return _mapper.Map<Tag>(createdTag);
    }

    /// <summary>
    /// Updates an existing tag with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to update.</param>
    /// <param name="updateTag">The updated tag data.</param>
    /// <returns>The updated tag details.</returns>
    public async Task<Tag> UpdateTagAsync(Guid id, Tag updateTag)
    {
        if (string.IsNullOrWhiteSpace(updateTag.Name))
        {
            throw new ArgumentException("Tag name is required", nameof(updateTag));
        }

        var existingTag = await _tagRepository.GetTagByIdAsync(id);
        if (existingTag == null)
        {
            throw new InvalidOperationException($"Tag with ID {id} not found");
        }

        // Check if another tag with the same name already exists
        var tagWithSameName = await _tagRepository.GetTagByNameAsync(updateTag.Name);
        if (tagWithSameName != null && tagWithSameName.Id != id)
        {
            throw new InvalidOperationException($"Tag with name '{updateTag.Name}' already exists");
        }

        existingTag.Name = updateTag.Name.Trim();
        existingTag.Description = updateTag.Description?.Trim();
        if (!string.IsNullOrEmpty(updateTag.Color))
        {
            existingTag.Color = updateTag.Color;
        }

        await _tagRepository.UpdateTagAsync(existingTag);
        return _mapper.Map<Tag>(existingTag);
    }

    /// <summary>
    /// Deletes a tag by its identifier if it's not being used by any agents.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete.</param>
    /// <returns>True if the tag was deleted, false otherwise.</returns>
    public async Task<bool> DeleteTagAsync(Guid id)
    {
        var tag = await _tagRepository.GetTagByIdAsync(id);
        if (tag == null)
        {
            return false;
        }

        // Check if tag is being used by any agents
        var isTagUsed = await _tagRepository.IsTagUsedByAgentsAsync(id);
        if (isTagUsed)
        {
            throw new InvalidOperationException("Cannot delete tag that is currently assigned to agents");
        }

        await _tagRepository.DeleteTagAsync(id);
        return true;
    }

    /// <summary>
    /// Searches for tags using a text query against name and description.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of tags matching the search criteria.</returns>
    public async Task<IEnumerable<Tag>> SearchTagsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetTagsAsync();
        }

        var tags = await _tagRepository.SearchTagsAsync(query);
        return _mapper.Map<IEnumerable<Tag>>(tags);
    }

    /// <summary>
    /// Gets a tag by name or creates it if it does not exist.
    /// </summary>
    /// <param name="tagName">The name of the tag to get or create.</param>
    /// <returns>The existing or newly created tag.</returns>
    public async Task<Tag> GetOrCreateTagAsync(string tagName)
    {
        var tag = await _tagRepository.GetOrCreateTagAsync(tagName);
        return _mapper.Map<Tag>(tag);
    }

    /// <summary>
    /// Checks whether the specified tag is used by any agents.
    /// </summary>
    /// <param name="tagId">The identifier of the tag to check.</param>
    /// <returns>True if the tag is used by agents, false otherwise.</returns>
    public async Task<bool> IsTagUsedByAgentsAsync(Guid tagId)
    {
        return await _tagRepository.IsTagUsedByAgentsAsync(tagId);
    }

    /// <summary>
    /// Helper method to generate a random color from a predefined palette for new tags.
    /// </summary>
    /// <returns>A hexadecimal color code.</returns>
    private static string GenerateRandomColor()
    {
        var colors = new[]
        {
            "#3B82F6", "#10B981", "#F59E0B", "#8B5CF6", "#EF4444", "#06B6D4", "#84CC16",
            "#F97316", "#EC4899", "#6366F1", "#14B8A6", "#F43F5E", "#A855F7", "#22C55E"
        };

        var random = new Random();
        return colors[random.Next(colors.Length)];
    }
}
