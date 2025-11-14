using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
///     Repository implementation for DocumentExport entities.
/// </summary>
public class DocumentExportRepository : IDocumentExportRepository
{
    private readonly AndyDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentExportRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public DocumentExportRepository(AndyDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<DocumentExportEntity?> GetByIdAsync(Guid id)
    {
        return await _context.DocumentExports
            .Include(d => d.CreatedByUser)
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <inheritdoc />
    public async Task<DocumentExportEntity?> GetByFileNameAsync(string fileName)
    {
        return await _context.DocumentExports
            .Include(d => d.CreatedByUser)
            .Include(d => d.Agent)
            .FirstOrDefaultAsync(d => d.FileName == fileName);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentExportEntity>> GetAllAsync()
    {
        return await _context.DocumentExports
            .Include(d => d.CreatedByUser)
            .Include(d => d.Agent)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentExportEntity>> GetByAgentIdAsync(Guid agentId)
    {
        return await _context.DocumentExports
            .Include(d => d.CreatedByUser)
            .Include(d => d.Agent)
            .Where(d => d.AgentId == agentId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentExportEntity>> GetByUserIdAsync(Guid userId)
    {
        return await _context.DocumentExports
            .Include(d => d.CreatedByUser)
            .Include(d => d.Agent)
            .Where(d => d.CreatedByUserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<DocumentExportEntity> AddAsync(DocumentExportEntity documentExport)
    {
        documentExport.Id = Guid.NewGuid();
        documentExport.CreatedAt = DateTime.UtcNow;

        _context.DocumentExports.Add(documentExport);
        await _context.SaveChangesAsync();
        return documentExport;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var documentExport = await _context.DocumentExports.FindAsync(id);
        if (documentExport == null)
        {
            return false;
        }

        _context.DocumentExports.Remove(documentExport);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.DocumentExports.AnyAsync(d => d.Id == id);
    }
}

