using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository implementation for managing LLM (Large Language Model) configuration entities in the database.
    /// Provides basic CRUD operations for LLM configurations including creation, retrieval, updates, and deletion.
    /// Manages configuration settings for various LLM providers and their associated parameters.
    /// </summary>
    public class LlmRepository(AndyDbContext context) : EfRepository<LlmConfigEntity>(context) ,ILlmRepository
    {

        public async Task<IEnumerable<LlmConfigEntity>> GetAllAsync()
        {
            return await context.LlmConfigs
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LlmConfigEntity?> GetByIdAsync(Guid id)
        {
            return await context.LlmConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<LlmConfigEntity> CreateAsync(LlmConfigEntity config)
        {
            config.Id = Guid.NewGuid();
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;

            context.LlmConfigs.Add(config);
            await context.SaveChangesAsync();

            return config;
        }

        public async Task<LlmConfigEntity> UpdateAsync(LlmConfigEntity config)
        {
            config.UpdatedAt = DateTime.UtcNow;
            context.LlmConfigs.Update(config);
            await context.SaveChangesAsync();

            return config;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var config = await context.LlmConfigs.FindAsync(id);
            if (config == null)
                return false;

            context.LlmConfigs.Remove(config);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

