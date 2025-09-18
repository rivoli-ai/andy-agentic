using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository implementation for managing Agent entities in the database.
    /// Provides specialized methods for agent operations including search, filtering by type and tags,
    /// and comprehensive data loading with related entities like LLM configurations, prompts, tools, and MCP servers.
    /// Inherits from EfRepository to provide basic CRUD operations.
    /// </summary>
    public class AgentRepository(AndyDbContext context) : EfRepository<AgentEntity>(context), IAgentRepository
    {
        public async Task<IEnumerable<AgentEntity>> GetAllAsync()
        {
            var opts = new QueryOptions<AgentEntity>
            {
                Page = 1,
                PageSize = 20,
                AsNoTracking = false,
                Includes = 
            {
                q => q.Include(o => o.LlmConfig),
                q => q.Include(o => o.EmbeddingLlmConfig),
                q => q.Include(o => o.AgentTags).ThenInclude(t=>t.Tag),
                q => q.Include(o => o.Prompts),
                q => q.Include(o => o.Tools),
                q => q.Include(o => o.McpServers),
                q => q.Include(o => o.AgentDocuments)
                    .ThenInclude(ad => ad.Document),
            }
            };


            var result = await ListAsync(opts);
            return result.Items;
        }

        public async Task<AgentEntity?> GetByIdAsync(Guid id)
        {
            var opts = new QueryOptions<AgentEntity>
            {
                Page = 1,
                PageSize = 20,
                AsNoTracking = false,
                Filters = {  a=>a.Id == id},
                Includes =
                {
                    q => q.Include(o => o.LlmConfig),
                    q => q.Include(o => o.EmbeddingLlmConfig),
                    q => q.Include(o => o.AgentTags)
                        .ThenInclude(t=>t.Tag),
                    q => q.Include(o => o.Prompts)
                        .ThenInclude(t=>t.Variables),
                    q => q.Include(o => o.Tools)
                        .ThenInclude(t=>t.Tool),
                    q => q.Include(o => o.McpServers),
                    q => q.Include(o => o.AgentDocuments)
                        .ThenInclude(ad => ad.Document),
                }
            };

            return await GetOneAsync(opts);

        }

        public async Task<AgentEntity> CreateAsync(AgentEntity agent)
        {
            context.Agents.Add(agent);
            await context.SaveChangesAsync();
            return agent;
        }

        public async Task<AgentEntity> UpdateAsync(AgentEntity agent)
        {
            await UpdateWithIncludesAsync(
                agent,
                a => a.Id == agent.Id,
                q => q
                    .Include(a => a.LlmConfig)
                    .Include(a => a.Prompts)
                    .Include(a => a.Tools)
                    .Include(a => a.McpServers)
                    .Include(a => a.AgentTags)
            );
            return agent;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var agent = await context.Agents.FindAsync(id);
            if (agent == null)
                return false;
            context.Agents.Remove(agent);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AgentEntity>> SearchAsync(string query)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Where(a => a.IsActive &&
                            (a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             a.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             a.Type.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AgentEntity>> GetByTypeAsync(string type)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Where(a => a.IsActive && a.Type == type)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<AgentEntity>> GetByTagAsync(string tag)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Where(a => a.IsActive && a.AgentTags.Any(at => at.Tag.Name == tag))
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AgentEntity?> GetByNameAsync(string name)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.Prompts)
                .Include(a => a.Tools)
                .Include(a => a.McpServers)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Name == name);
        }

        public async Task<AgentEntity?> GetWithConfigAsync(Guid id)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.Prompts.Where(p => p.IsActive))
                .Include(a => a.Tools)
                .ThenInclude(at => at.Tool)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AgentEntity>> GetVisibleAsync(Guid userId)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a => a.EmbeddingLlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.Prompts)
                .Include(a => a.Tools)
                .ThenInclude(at => at.Tool)
                .Where(a => a.IsPublic || a.CreatedByUserId == userId)
                .Include(at => at.AgentDocuments)
                .ThenInclude(d => d.Document)
                .OrderBy(a => a.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<AgentEntity?> GetVisibleByIdAsync(Guid id, Guid userId)
        {
            return await context.Agents
                .Include(a => a.LlmConfig)
                .Include(a=>a.EmbeddingLlmConfig)
                .Include(a => a.AgentTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.Prompts)
                .Include(a => a.Tools)
                .ThenInclude(at => at.Tool)
                .Include(a => a.McpServers)
                .Include(at => at.AgentDocuments)
                .ThenInclude(d=>d.Document)
                .Where(a => a.Id == id && (a.IsPublic || a.CreatedByUserId == userId))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
