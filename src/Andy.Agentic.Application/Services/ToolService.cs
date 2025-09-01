using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using AutoMapper;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Service for managing tool operations including CRUD operations, search, and filtering.
///     Acts as a facade over the database service for tool-related functionality.
/// </summary>
public class ToolService(IDataBaseService databaseResourceAccess, IMapper mapper) : IToolService
{
    /// <summary>
    ///     Retrieves all tools from the database.
    /// </summary>
    /// <returns>A collection of all tool s.</returns>
    public async Task<IEnumerable<Tool>> GetAllToolsAsync() => await databaseResourceAccess.GetAllToolsAsync();

    /// <summary>
    ///     Retrieves a specific tool by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tool.</param>
    /// <returns>The tool  if found; otherwise, null.</returns>
    public async Task<Tool?> GetToolByIdAsync(Guid id) => await databaseResourceAccess.GetToolByIdAsync(id);

    /// <summary>
    ///     Creates a new tool in the database.
    /// </summary>
    /// <param name="createTool">The tool data for creation.</param>
    /// <returns>The created tool .</returns>
    public async Task<Tool> CreateToolAsync(Tool createTool) =>
        await databaseResourceAccess.CreateToolAsync(createTool);

    /// <summary>
    ///     Updates an existing tool in the database.
    /// </summary>
    /// <param name="id">The unique identifier of the tool to update.</param>
    /// <param name="updateTool">The updated tool data.</param>
    /// <returns>The updated tool .</returns>
    public async Task<Tool> UpdateToolAsync(Guid id, Tool updateTool) =>
        await databaseResourceAccess.UpdateToolAsync(id, updateTool);

    /// <summary>
    ///     Deletes a tool from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the tool to delete.</param>
    /// <returns>True if the tool was successfully deleted; false if not found.</returns>
    public async Task<bool> DeleteToolAsync(Guid id) => await databaseResourceAccess.DeleteToolAsync(id);

    /// <summary>
    ///     Searches for tools using a free-text query.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A collection of tool s matching the search criteria.</returns>
    public async Task<IEnumerable<Tool>> SearchToolsAsync(string query) =>
        await databaseResourceAccess.SearchToolsAsync(query);

    /// <summary>
    ///     Retrieves tools filtered by their category.
    /// </summary>
    /// <param name="category">The tool category to filter by.</param>
    /// <returns>A collection of tool s in the specified category.</returns>
    public async Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category) =>
        await databaseResourceAccess.GetToolsByCategoryAsync(category);

    /// <summary>
    ///     Retrieves tools filtered by their type.
    /// </summary>
    /// <param name="type">The tool type to filter by.</param>
    /// <returns>A collection of tool s of the specified type.</returns>
    public async Task<IEnumerable<Tool>> GetToolsByTypeAsync(string type) =>
        await databaseResourceAccess.GetToolsByTypeAsync(type);

    /// <summary>
    ///     Retrieves only active tools that are available for use.
    /// </summary>
    /// <returns>A collection of active tool s.</returns>
    public async Task<IEnumerable<Tool>> GetActiveToolsAsync() => await databaseResourceAccess.GetActiveToolsAsync();
}
