using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository implementation for managing Tool entities in the database.
    /// Provides specialized methods for tool operations including search, filtering by category and type,
    /// validation of tool usage by agents, and comprehensive tool management functionality.
    /// Implements business logic to prevent deletion of tools that are currently in use by agents.
    /// </summary>
    public class ToolRepository(AndyDbContext context) : IToolRepository
    {
        public async Task<IEnumerable<ToolEntity>> GetAllAsync()
        {
            return await context.Tools
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<ToolEntity?> GetByIdAsync(Guid id)
        {
            return await context.Tools.FindAsync(id);
        }

        public async Task<ToolEntity> CreateAsync(ToolEntity tool)
        {
            tool.CreatedAt = DateTime.UtcNow;
            tool.UpdatedAt = DateTime.UtcNow;
            context.Tools.Add(tool);
            await context.SaveChangesAsync();
            return tool;
        }

        public async Task<ToolEntity> UpdateAsync(ToolEntity tool)
        {
            tool.UpdatedAt = DateTime.UtcNow;
            context.Tools.Update(tool);
            await context.SaveChangesAsync();
            return tool;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var tool = await context.Tools.FindAsync(id);
            if (tool == null)
                return false;

            // Check if tool is being used by any agents
            var isUsedByAgents = await context.AgentTools
                .Include(x=>x.Agent)
                .AnyAsync(x=>x.Agent.IsActive);

            if (isUsedByAgents)
                throw new InvalidOperationException($"Cannot delete tool '{tool.Name}' as it is currently being used by agents");

            context.Tools.Remove(tool);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ToolEntity>> SearchAsync(string query)
        {
            return await context.Tools
                .Where(t => t.IsActive && 
                           (t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            t.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            t.Category != null && t.Category.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolEntity>> GetByCategoryAsync(string category)
        {
            return await context.Tools
                .Where(t => t.IsActive && t.Category == category)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolEntity>> GetByTypeAsync(string type)
        {
            return await context.Tools
                .Where(t => t.IsActive && t.Type == type)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolEntity>> GetActiveAsync()
        {
            return await context.Tools
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

    }
}

