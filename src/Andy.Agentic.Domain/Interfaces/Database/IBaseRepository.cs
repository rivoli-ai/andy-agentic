using System.Linq.Expressions;
using Andy.Agentic.Domain.Queries.Pagination;
using Andy.Agentic.Domain.Queries.SearchCriteria;

namespace Andy.Agentic.Domain.Interfaces.Database;

/// <summary>
///     Defines base repository operations for CRUD and advanced querying.
/// </summary>
public interface IBaseRepository<T> where T : class
{
    // 🔹 Basic CRUD
    /// <summary>
    ///     Gets an entity by primary key.
    /// </summary>
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);

    /// <summary>
    ///     Gets a single entity matching the predicate, or null if none.
    /// </summary>
    Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    ///     Determines whether any entity matches the predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    ///     Counts entities matching an optional predicate.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    ///     Adds a new entity to the data store.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    ///     Adds multiple entities to the data store.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>
    ///     Updates an existing entity.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>
    ///     Deletes an entity instance.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken ct = default);

    /// <summary>
    ///     Deletes an entity by primary key.
    /// </summary>
    Task DeleteByIdAsync(object id, CancellationToken ct = default);

    // 🔹 Advanced querying
    /// <summary>
    ///     Lists entities according to the specified query options, including filtering, includes, sorting and pagination.
    /// </summary>
    Task<PaginatedResult<T>> ListAsync(QueryOptions<T> options, CancellationToken ct = default);

    // 🔹 Update with includes (full graph update)
    /// <summary>
    ///     Updates an entity while including and tracking a related graph for full synchronization.
    /// </summary>
    Task UpdateWithIncludesAsync(
        T updatedEntity,
        Expression<Func<T, bool>> matchPredicate,
        Func<IQueryable<T>, IQueryable<T>> includeGraph,
        CancellationToken ct = default
    );
}
