using Andy.Agentic.Domain.Entities;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Provides data access operations for <see cref="ToolEntity" />.
/// </summary>
public interface IToolRepository
{
    /// <summary>
    ///     Retrieves all tools.
    /// </summary>
    Task<IEnumerable<ToolEntity>> GetAllAsync();

    /// <summary>
    ///     Retrieves a tool by identifier.
    /// </summary>
    Task<ToolEntity?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Creates a new tool.
    /// </summary>
    Task<ToolEntity> CreateAsync(ToolEntity tool);

    /// <summary>
    ///     Updates an existing tool.
    /// </summary>
    Task<ToolEntity> UpdateAsync(ToolEntity tool);

    /// <summary>
    ///     Deletes a tool by identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Searches tools by free-text query.
    /// </summary>
    Task<IEnumerable<ToolEntity>> SearchAsync(string query);

    /// <summary>
    ///     Gets tools by category.
    /// </summary>
    Task<IEnumerable<ToolEntity>> GetByCategoryAsync(string category);

    /// <summary>
    ///     Gets tools by type.
    /// </summary>
    Task<IEnumerable<ToolEntity>> GetByTypeAsync(string type);

    /// <summary>
    ///     Gets currently active tools.
    /// </summary>
    Task<IEnumerable<ToolEntity>> GetActiveAsync();
}
