using Andy.Agentic.Domain.Entities;
using Andy.Agentic.Domain.Interfaces.Database;
using Andy.Agentic.Domain.Queries.Pagination;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
/// Generic Entity Framework repository implementation providing common CRUD operations
/// for any entity type. Implements basic database operations including create, read,
/// update, delete, and paginated queries with filtering and sorting capabilities.
/// </summary>
/// <typeparam name="T">The type of entity this repository manages.</typeparam>
public class EfRepository<T> : IBaseRepository<T> where T : class 
{
    private readonly DbContext _db;

    /// <summary>
    /// Gets the DbSet for the entity type T, providing access to Entity Framework querying capabilities.
    /// </summary>
    protected DbSet<T> Set => _db.Set<T>();

    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository{T}"/> class.
    /// </summary>
    /// <param name="db">The Entity Framework database context.</param>
    public EfRepository(DbContext db) => _db = db;

    /// <summary>
    /// Retrieves a single entity based on the specified query options asynchronously.
    /// </summary>
    /// <param name="id">the item id.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>The first entity matching the criteria, or null if none found.</returns>
    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    public Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();

    /// <summary>
    /// Retrieves a single entity based on the specified query options asynchronously.
    /// </summary>
    /// <param name="options">The query options including filters, includes, search, and sorting.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>The first entity matching the criteria, or null if none found.</returns>
    public async Task<T?> GetOneAsync(QueryOptions<T> options, CancellationToken ct = default)
    {
        var baseQuery = Set.AsQueryable();

        var filteredQuery = baseQuery.ApplyQueryCore(options);

        return await filteredQuery.FirstOrDefaultAsync(cancellationToken: ct);
    }

    /// <summary>
    /// Checks if any entity exists that matches the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">The predicate expression to test against entities.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>True if any entity matches the predicate, otherwise false.</returns>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Set.AsNoTracking().AnyAsync(predicate, ct);

    /// <summary>
    /// Counts the total number of entities, optionally filtered by a predicate, asynchronously.
    /// </summary>
    /// <param name="predicate">Optional predicate expression to filter entities before counting.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>The total count of entities matching the criteria.</returns>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        var q = Set.AsNoTracking().AsQueryable();
        if (predicate != null)
        {
            q = q.Where(predicate);
        }

        return await q.CountAsync(ct);
    }

    /// <summary>
    /// Adds a new entity to the database asynchronously and saves changes.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>The added entity with any generated values (e.g., ID).</returns>
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// Adds multiple entities to the database asynchronously and saves changes.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await Set.AddRangeAsync(entities, ct);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatedEntity"></param>
    /// <param name="matchPredicate"></param>
    /// <param name="includeGraph"></param>
    /// <param name="ct"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task UpdateWithIncludesAsync(
        T updatedEntity,
        Expression<Func<T, bool>> matchPredicate,
        Func<IQueryable<T>, IQueryable<T>> includeGraph,
        CancellationToken ct = default)
    {
        var dbSet = _db.Set<T>();

        // 1. Load existing entity with includes
        var existingEntity = await includeGraph(dbSet)
            .FirstOrDefaultAsync(matchPredicate, ct);

        if (existingEntity == null)
            throw new KeyNotFoundException($"{typeof(T).Name} not found");

        // 2. Update scalar properties
        _db.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

        // 3. Sync navigation properties
        foreach (var nav in _db.Entry(existingEntity).Navigations)
        {
            var navName = nav.Metadata.Name;
            var updatedNav = _db.Entry(updatedEntity).Member(navName).CurrentValue;

            if (nav.Metadata.IsCollection)
            {
                var existingCollection = (IEnumerable<object>)nav.CurrentValue!;
                var updatedCollection = (IEnumerable<object>)updatedNav!;

                SyncCollection(existingCollection, updatedCollection, existingEntity);
            }
            else
            {
                if (updatedNav != null)
                {
                    _db.Entry(updatedNav).State = EntityState.Modified;
                    nav.CurrentValue = updatedNav;
                }
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private void SyncCollection(IEnumerable<object> existing, IEnumerable<object> updated, object parent)
    {
        var existingList = existing.ToList();
        var updatedList = updated.ToList();

        // Remove missing
        foreach (var e in existingList)
        {
            if (!updatedList.Any(u => IsSameEntity(u, e)))
                _db.Remove(e);
        }

        // Add or update
        foreach (var u in updatedList)
        {
            var match = existingList.FirstOrDefault(e => IsSameEntity(e, u));
            if (match == null)
            {
                FixForeignKeys(u, parent);
                _db.Add(u);
            }
            else
            {
                _db.Entry(match).CurrentValues.SetValues(u);
            }
        }
    }

    private void FixForeignKeys(object child, object parent)
    {
        var parentType = parent.GetType();
        var childType = child.GetType();

        var navs = _db.Model.FindEntityType(childType)!.GetForeignKeys();

        foreach (var fk in navs)
        {
            if (fk.PrincipalEntityType.ClrType == parentType)
            {
                var principalKey = fk.PrincipalKey.Properties.First().Name;
                var dependentProp = fk.Properties.First().Name;

                var keyValue = parentType.GetProperty(principalKey)!.GetValue(parent);
                childType.GetProperty(dependentProp)!.SetValue(child, keyValue);
            }
        }
    }

    /// <summary>
    /// Updates an existing entity in the database asynchronously and saves changes.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deletes an entity from the database asynchronously and saves changes.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        Set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deletes an entity by its primary key identifier asynchronously and saves changes.
    /// </summary>
    /// <param name="id">The primary key identifier of the entity to delete.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteByIdAsync(object id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is null)
        {
            return;
        }

        await DeleteAsync(entity, ct);
    }

    /// <summary>
    /// Retrieves a paginated list of entities based on the specified query options asynchronously.
    /// </summary>
    /// <param name="options">The query options including filters, includes, search, sorting, and pagination.</param>
    /// <param name="ct">Optional cancellation token to cancel the operation.</param>
    /// <returns>A paginated result containing the entities and pagination metadata.</returns>
    public async Task<PaginatedResult<T>> ListAsync(QueryOptions<T> options, CancellationToken ct = default)
    {
        var baseQuery = Set.AsQueryable();

        var filteredQuery = baseQuery.ApplyQueryCore(options);

        var total = await filteredQuery.CountAsync(ct);

        var paged = filteredQuery.ApplyPagination(options);

        var items = await paged.ToListAsync(ct);

        var page = options.Page ?? 1;
        var pageSize = options.PageSize ?? total;

        return new PaginatedResult<T>(items, total, page, pageSize);
    }


    private bool IsSameEntity(object a, object b)
    {
        var type = a.GetType();
        var key = _db.Model.FindEntityType(type)!.FindPrimaryKey()!;
        return key.Properties.All(p =>
        {
            var valA = type.GetProperty(p.Name)!.GetValue(a);
            var valB = type.GetProperty(p.Name)!.GetValue(b);
            return Equals(valA, valB);
        });
    }

}
