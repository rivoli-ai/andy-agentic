using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
/// Repository implementation for managing Tag entities and their relationships with agents in the database.
/// Provides functionality for tag creation, retrieval, and management of agent-tag associations.
/// Implements lazy tag creation and synchronization logic for maintaining tag collections
/// associated with agents.
/// </summary>
public class TagRepository(AndyDbContext context) : ITagRepository
{
    public async Task<TagEntity> GetOrCreateTagAsync(string tagName)
    {
        var tag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        if (tag != null) return tag;

        tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = tagName,
            CreatedAt = DateTime.UtcNow
        };
        context.Tags.Add(tag);

        return tag;
    }

    public async Task UpdateTagsAsync(AgentEntity agent, List<string>? tagNames)
    {
        if (tagNames == null || !tagNames.Any()) return;

        context.AgentTags.RemoveRange(agent.AgentTags);

        foreach (var tagName in tagNames)
        {
            var tag = await GetOrCreateTagAsync(tagName);
            var agentTag = new AgentTagEntity
            {
                Id = Guid.NewGuid(),
                AgentId = agent.Id,
                Tag = tag
            };

            context.AgentTags.Add(agentTag);
        }
    }

    public async Task<IEnumerable<TagEntity>> GetAllTagsAsync()
    {
        return await context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<TagEntity?> GetTagByIdAsync(Guid id)
    {
        return await context.Tags.FindAsync(id);
    }

    public async Task<TagEntity?> GetTagByNameAsync(string name)
    {
        return await context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<TagEntity> CreateTagAsync(TagEntity tag)
    {
        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        return tag;
    }

    public async Task UpdateTagAsync(TagEntity tag)
    {
        context.Tags.Update(tag);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(Guid id)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TagEntity>> SearchTagsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllTagsAsync();

        return await context.Tags
            .Where(t => t.Name.ToLower().Contains(query.ToLower()) ||
                        (t.Description != null && t.Description.ToLower().Contains(query.ToLower())))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<bool> IsTagUsedByAgentsAsync(Guid tagId)
    {
        return await context.AgentTags.AnyAsync(at => at.TagId == tagId);
    }
}
