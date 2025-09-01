using Andy.Agentic.Application.DTOs;
using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

/// <summary>
/// Provides operations for tag management and business logic.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Retrieves all available tags.
    /// </summary>
    /// <returns>A list of all tags in the system.</returns>
    Task<IEnumerable<Tag>> GetTagsAsync();

    /// <summary>
    /// Retrieves a specific tag by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag.</param>
    /// <returns>The tag details if found, null otherwise.</returns>
    Task<Tag?> GetTagByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a tag by name.
    /// </summary>
    /// <param name="name">The name of the tag to find.</param>
    /// <returns>The tag details if found, null otherwise.</returns>
    Task<Tag?> GetTagByNameAsync(string name);

    /// <summary>
    /// Creates a new tag with the provided information.
    /// </summary>
    /// <param name="createTagDto">The tag data for creation.</param>
    /// <returns>The created tag details.</returns>
    Task<Tag> CreateTagAsync(Tag createTagDto);

    /// <summary>
    /// Updates an existing tag with new information.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to update.</param>
    /// <param name="updateTagDto">The updated tag data.</param>
    /// <returns>The updated tag details.</returns>
    Task<Tag> UpdateTagAsync(Guid id, Tag updateTagDto);

    /// <summary>
    /// Deletes a tag by its identifier if it's not being used by any agents.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete.</param>
    /// <returns>True if the tag was deleted, false otherwise.</returns>
    Task<bool> DeleteTagAsync(Guid id);

    /// <summary>
    /// Searches for tags using a text query against name and description.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of tags matching the search criteria.</returns>
    Task<IEnumerable<Tag>> SearchTagsAsync(string query);

    /// <summary>
    /// Gets a tag by name or creates it if it does not exist.
    /// </summary>
    /// <param name="tagName">The name of the tag to get or create.</param>
    /// <returns>The existing or newly created tag.</returns>
    Task<Tag> GetOrCreateTagAsync(string tagName);

    /// <summary>
    /// Checks whether the specified tag is used by any agents.
    /// </summary>
    /// <param name="tagId">The identifier of the tag to check.</param>
    /// <returns>True if the tag is used by agents, false otherwise.</returns>
    Task<bool> IsTagUsedByAgentsAsync(Guid tagId);
}
