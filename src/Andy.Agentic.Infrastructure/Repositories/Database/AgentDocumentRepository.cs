using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
///     Repository implementation for Agent-Document relationships.
/// </summary>
public class AgentDocumentRepository : IAgentDocumentRepository
{
    private readonly AndyDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentDocumentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AgentDocumentRepository(AndyDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AgentDocumentEntity>> GetAllAsync()
    {
        return await _context.AgentDocuments
            .Include(ad => ad.Agent)
            .Include(ad => ad.Document)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AgentDocumentEntity>> GetByAgentIdAsync(Guid agentId)
    {
        return await _context.AgentDocuments
            .Include(ad => ad.Agent)
            .Include(ad => ad.Document)
            .Where(ad => ad.AgentId == agentId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AgentDocumentEntity>> GetByDocumentIdAsync(Guid documentId)
    {
        return await _context.AgentDocuments
            .Include(ad => ad.Agent)
            .Include(ad => ad.Document)
            .Where(ad => ad.DocumentId == documentId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AgentDocumentEntity?> GetByAgentAndDocumentIdAsync(Guid agentId, Guid documentId)
    {
        return await _context.AgentDocuments
            .Include(ad => ad.Agent)
            .Include(ad => ad.Document)
            .FirstOrDefaultAsync(ad => ad.AgentId == agentId && ad.DocumentId == documentId);
    }

    /// <inheritdoc />
    public async Task<AgentDocumentEntity> AddAsync(AgentDocumentEntity agentDocument)
    {
        _context.AgentDocuments.Add(agentDocument);
        await _context.SaveChangesAsync();
        return agentDocument;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var agentDocument = await _context.AgentDocuments.FindAsync(id);
        if (agentDocument == null)
            return false;

        _context.AgentDocuments.Remove(agentDocument);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteByAgentAndDocumentIdAsync(Guid agentId, Guid documentId)
    {
        var agentDocument = await _context.AgentDocuments
            .FirstOrDefaultAsync(ad => ad.AgentId == agentId && ad.DocumentId == documentId);
        
        if (agentDocument == null)
            return false;

        _context.AgentDocuments.Remove(agentDocument);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid agentId, Guid documentId)
    {
        return await _context.AgentDocuments
            .AnyAsync(ad => ad.AgentId == agentId && ad.DocumentId == documentId);
    }
}
