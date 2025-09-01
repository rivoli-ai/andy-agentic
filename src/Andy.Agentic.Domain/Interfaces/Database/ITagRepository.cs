using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides operations for tag management and associations with agents.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    ///     Gets a tag by name or creates it if it does not exist.
    /// </summary>
    Task<TagEntity> GetOrCreateTagAsync(string tagName);

    /// <summary>
    ///     Replaces tags assigned to an agent with the provided list.
    /// </summary>
    Task UpdateTagsAsync(AgentEntity existingAgent, List<string>? tags);

    /// <summary>
    ///     Retrieves all tags.
    /// </summary>
    Task<IEnumerable<TagEntity>> GetAllTagsAsync();

    /// <summary>
    ///     Retrieves a tag by identifier.
    /// </summary>
    Task<TagEntity?> GetTagByIdAsync(Guid id);

    /// <summary>
    ///     Retrieves a tag by name.
    /// </summary>
    Task<TagEntity?> GetTagByNameAsync(string name);

    /// <summary>
    ///     Creates a new tag.
    /// </summary>
    Task<TagEntity> CreateTagAsync(TagEntity tag);

    /// <summary>
    ///     Updates an existing tag.
    /// </summary>
    Task UpdateTagAsync(TagEntity tag);

    /// <summary>
    ///     Deletes a tag by identifier.
    /// </summary>
    Task DeleteTagAsync(Guid id);

    /// <summary>
    ///     Searches for tags by free-text query.
    /// </summary>
    Task<IEnumerable<TagEntity>> SearchTagsAsync(string query);

    /// <summary>
    ///     Checks whether the specified tag is used by any agents.
    /// </summary>
    Task<bool> IsTagUsedByAgentsAsync(Guid tagId);
}
