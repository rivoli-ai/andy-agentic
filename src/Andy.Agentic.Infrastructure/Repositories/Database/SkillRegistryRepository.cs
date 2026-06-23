using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database
{
    /// <summary>
    /// Repository for skill registry connections and per-agent skill attachments.
    /// </summary>
    public class SkillRegistryRepository(AndyDbContext context) : ISkillRegistryRepository
    {
        public async Task<IEnumerable<SkillRegistryEntity>> GetAllAsync()
        {
            return await context.SkillRegistries
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<SkillRegistryEntity?> GetByIdAsync(Guid id)
        {
            return await context.SkillRegistries.FindAsync(id);
        }

        public async Task<SkillRegistryEntity> CreateAsync(SkillRegistryEntity registry)
        {
            if (registry.Id == Guid.Empty)
            {
                registry.Id = Guid.NewGuid();
            }

            registry.CreatedAt = DateTime.UtcNow;
            registry.UpdatedAt = DateTime.UtcNow;
            context.SkillRegistries.Add(registry);
            await context.SaveChangesAsync();
            return registry;
        }

        public async Task<SkillRegistryEntity> UpdateAsync(SkillRegistryEntity registry)
        {
            registry.UpdatedAt = DateTime.UtcNow;
            context.SkillRegistries.Update(registry);
            await context.SaveChangesAsync();
            return registry;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var registry = await context.SkillRegistries.FindAsync(id);
            if (registry == null)
            {
                return false;
            }

            var inUse = await context.AgentSkills.AnyAsync(s => s.SkillRegistryId == id);
            if (inUse)
            {
                throw new InvalidOperationException(
                    $"Cannot delete registry '{registry.Name}' as skills from it are attached to agents");
            }

            context.SkillRegistries.Remove(registry);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AgentSkillEntity>> GetAgentSkillsAsync(Guid agentId)
        {
            return await context.AgentSkills
                .Include(s => s.Registry)
                .Where(s => s.AgentId == agentId)
                .OrderBy(s => s.DisplayName)
                .ToListAsync();
        }

        public async Task<AgentSkillEntity> AttachSkillAsync(AgentSkillEntity agentSkill)
        {
            if (agentSkill.Id == Guid.Empty)
            {
                agentSkill.Id = Guid.NewGuid();
            }

            agentSkill.CreatedAt = DateTime.UtcNow;
            agentSkill.Registry = null!;
            context.AgentSkills.Add(agentSkill);
            await context.SaveChangesAsync();
            return agentSkill;
        }

        public async Task<bool> DetachSkillAsync(Guid agentId, Guid agentSkillId)
        {
            var agentSkill = await context.AgentSkills
                .FirstOrDefaultAsync(s => s.Id == agentSkillId && s.AgentId == agentId);
            if (agentSkill == null)
            {
                return false;
            }

            context.AgentSkills.Remove(agentSkill);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
