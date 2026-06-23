using System.Linq.Expressions;

namespace Andy.Agentic.Domain.Queries.SearchCriteria;

/// <summary>
///     Configuration options for database queries including filtering, searching, sorting, pagination, and tracking.
///     Provides a flexible way to configure complex database queries with multiple criteria.
/// </summary>
public class QueryOptions<T>
{
    /// <summary>
    ///     Gets the list of filter expressions to apply to the query.
    ///     Each expression should return a boolean value for filtering.
    /// </summary>
    public List<Expression<Func<T, bool>>> Filters { get; } = new();

    /// <summary>
    ///     Gets the list of include expressions to load related entities.
    ///     Each expression should specify a navigation property to include.
    /// </summary>
    public List<Func<IQueryable<T>, IQueryable<T>>> Includes { get; } = new();

    /// <summary>
    ///     Gets or sets the free-text search term to search across multiple string fields.
    ///     The search is case-insensitive and looks for partial matches.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    ///     Gets the list of property selectors to use for free-text search.
    ///     Each selector should return a string property to search within.
    /// </summary>
    public List<Expression<Func<T, string?>>> SearchSelectors { get; } = new();

    /// <summary>
    ///     Gets or sets the ordering function to apply to the query.
    ///     Should provide a lambda for OrderBy/ThenBy operations.
    /// </summary>
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }

    /// <summary>
    ///     Gets or sets the page number for pagination (1-based indexing).
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    ///     Gets or sets the number of items per page for pagination.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    ///     Gets or sets whether to use AsNoTracking for better performance when entities won't be modified.
    ///     Defaults to true for better performance.
    /// </summary>
    public bool AsNoTracking { get; set; } = true;
}
