using Andy.Agentic.Domain.Models;

namespace Andy.Agentic.Application.Interfaces;

public interface IToolService
{
    Task<IEnumerable<Tool>> GetAllToolsAsync();
    Task<Tool?> GetToolByIdAsync(Guid id);
    Task<Tool> CreateToolAsync(Tool createTool);
    Task<Tool> UpdateToolAsync(Guid id, Tool updateTool);
    Task<bool> DeleteToolAsync(Guid id);
    Task<IEnumerable<Tool>> SearchToolsAsync(string query);
    Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category);
    Task<IEnumerable<Tool>> GetToolsByTypeAsync(string type);
    Task<IEnumerable<Tool>> GetActiveToolsAsync();
}
