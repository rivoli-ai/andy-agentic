using Andy.Agentic.Domain.Interfaces;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Andy.Agentic.Infrastructure.UnitOfWorks;

/// <summary>
/// Unit of Work implementation that manages database transactions and coordinates multiple repository operations.
/// Provides a centralized way to manage database transactions, ensuring data consistency across multiple
/// repository operations. Implements the Unit of Work pattern for transaction management.
/// </summary>
public sealed class UnitOfWork(
    AndyDbContext context,
    IAgentRepository agents,
    IPromptRepository prompts,
    IToolRepository tools,
    IMcpRepository mcps,
    ITagRepository tags)
    : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Gets the agent repository for managing agent-related operations.
    /// </summary>
    public IAgentRepository Agents { get; } = agents;

    /// <summary>
    /// Gets the prompt repository for managing prompt-related operations.
    /// </summary>
    public IPromptRepository Prompts { get; } = prompts;

    /// <summary>
    /// Gets the tool repository for managing tool-related operations.
    /// </summary>
    public IToolRepository Tools { get; } = tools;

    /// <summary>
    /// Gets the MCP server repository for managing MCP server-related operations.
    /// </summary>
    public IMcpRepository McpServers { get; } = mcps;

    /// <summary>
    /// Gets the tag repository for managing tag-related operations.
    /// </summary>
    public ITagRepository Tags { get; } = tags;

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already started.</exception>
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started.");

        _transaction = await context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Commits the current database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no active transaction to commit.</exception>
    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <summary>
    /// Rolls back the current database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there is no active transaction to rollback.</exception>
    public async Task RollbackAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to rollback.");

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <summary>
    /// Saves all pending changes to the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation with the number of affected entries.</returns>
    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();

    /// <summary>
    /// Disposes of the unit of work and any active transactions asynchronously.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await context.DisposeAsync();
    }
}
