using Andy.Agentic.Application.Interfaces;
using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Models;
using Mapster;

namespace Andy.Agentic.Application.Services;

/// <summary>
///     Manages skill registry connections and per-agent skill attachments, and proxies registry
///     search so the SPA never holds registry credentials.
/// </summary>
public class SkillManager(ISkillRegistryRepository repository, ISkillRegistryClient registryClient) : ISkillManager
{
    public async Task<IEnumerable<SkillRegistry>> GetRegistriesAsync()
    {
        var entities = await repository.GetAllAsync();
        return entities.Adapt<List<SkillRegistry>>();
    }

    public async Task<SkillRegistry?> GetRegistryAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        return entity?.Adapt<SkillRegistry>();
    }

    public async Task<SkillRegistry> CreateRegistryAsync(SkillRegistry registry)
    {
        var entity = registry.Adapt<SkillRegistryEntity>();
        var created = await repository.CreateAsync(entity);
        return created.Adapt<SkillRegistry>();
    }

    public async Task<SkillRegistry> UpdateRegistryAsync(SkillRegistry registry)
    {
        var existing = await repository.GetByIdAsync(registry.Id)
            ?? throw new KeyNotFoundException($"Skill registry '{registry.Id}' not found");

        existing.Name = registry.Name;
        existing.Description = registry.Description;
        existing.BaseUrl = registry.BaseUrl;
        existing.AuthType = registry.AuthType;
        existing.IsActive = registry.IsActive;

        // Only overwrite stored credentials when the caller supplies new ones (write-only secret).
        if (!string.IsNullOrEmpty(registry.AuthConfig))
        {
            existing.AuthConfig = registry.AuthConfig;
        }

        var updated = await repository.UpdateAsync(existing);
        return updated.Adapt<SkillRegistry>();
    }

    public Task<bool> DeleteRegistryAsync(Guid id) => repository.DeleteAsync(id);

    public async Task<bool> TestRegistryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registry = await GetRegistryOrThrowAsync(id);
        return await registryClient.TestConnectionAsync(registry, cancellationToken);
    }

    public async Task<IReadOnlyList<SkillSearchResult>> SearchAsync(Guid registryId, string query, CancellationToken cancellationToken = default)
    {
        var registry = await GetRegistryOrThrowAsync(registryId);
        return await registryClient.SearchAsync(registry, query, cancellationToken);
    }

    public async Task<IEnumerable<AgentSkill>> GetAgentSkillsAsync(Guid agentId)
    {
        var entities = await repository.GetAgentSkillsAsync(agentId);
        return entities.Adapt<List<AgentSkill>>();
    }

    public async Task<AgentSkill> AttachSkillAsync(Guid agentId, AgentSkill skill)
    {
        var registry = await GetRegistryOrThrowAsync(skill.SkillRegistryId);

        // Resolve display name/description from the registry if the caller did not provide them.
        if (string.IsNullOrWhiteSpace(skill.DisplayName))
        {
            skill.DisplayName = skill.SkillSlug;
        }

        var entity = new AgentSkillEntity
        {
            AgentId = agentId,
            SkillRegistryId = registry.Id,
            Namespace = skill.Namespace,
            SkillSlug = skill.SkillSlug,
            Version = skill.Version,
            DisplayName = skill.DisplayName,
            Description = skill.Description,
            IsActive = skill.IsActive,
        };

        var created = await repository.AttachSkillAsync(entity);
        return created.Adapt<AgentSkill>();
    }

    public Task<bool> DetachSkillAsync(Guid agentId, Guid agentSkillId) =>
        repository.DetachSkillAsync(agentId, agentSkillId);

    private async Task<SkillRegistry> GetRegistryOrThrowAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Skill registry '{id}' not found");
        return entity.Adapt<SkillRegistry>();
    }
}
