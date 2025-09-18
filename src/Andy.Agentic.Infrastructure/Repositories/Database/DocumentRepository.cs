using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
///     Repository implementation for Document entities.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly AndyDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentRepository(AndyDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentEntity>> GetAllAsync()
    {
        return await _context.Documents
            .Include(d => d.CreatedByUser)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<DocumentEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.CreatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentEntity>> GetByAgentIdAsync(Guid agentId)
    {
        return await _context.Documents
            .Include(d => d.CreatedByUser)
            .Include(d => d.AgentDocuments)
            .Where(d => d.AgentDocuments.Any(ad => ad.AgentId == agentId))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<DocumentEntity> AddAsync(DocumentEntity document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    /// <inheritdoc />
    public async Task<DocumentEntity> UpdateAsync(DocumentEntity document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return false;

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id);
    }
}
