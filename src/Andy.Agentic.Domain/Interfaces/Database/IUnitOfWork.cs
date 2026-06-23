namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Represents a unit of work coordinating transactional operations across repositories.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    ///     Repository for agent entities.
    /// </summary>
    IAgentRepository Agents { get; }

    /// <summary>
    ///     Repository for prompt entities.
    /// </summary>
    IPromptRepository Prompts { get; }

    /// <summary>
    ///     Repository for tool entities.
    /// </summary>
    IToolRepository Tools { get; }

    /// <summary>
    ///     Repository for MCP server associations.
    /// </summary>
    IMcpRepository McpServers { get; }

    /// <summary>
    ///     Repository for tag entities and associations.
    /// </summary>
    ITagRepository Tags { get; }

    /// <summary>
    ///     Begins a database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    ///     Commits the current transaction.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    ///     Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    ///     Persists changes to the data store.
    /// </summary>
    Task<int> SaveChangesAsync();
}
