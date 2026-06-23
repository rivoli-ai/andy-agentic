using System.Linq.Expressions;
using Andy.Agentic.Domain.Queries.SearchCriteria;
using Microsoft.EntityFrameworkCore;

namespace Andy.Agentic.Infrastructure.Repositories.Database;

/// <summary>
/// Extension methods for IQueryable to apply query options including includes, filters, search, sorting, pagination, and tracking.
/// Provides a fluent API for building complex database queries with QueryOptions configuration.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    ///  Applies includes expressions to the query to load related entities.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply includes to.</param>
    /// <param name="options">The query options containing include expressions.</param>
    /// <returns>The queryable with includes applied.</returns>
    public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, QueryOptions<T> options)
        where T : class
    {
        foreach (var inc in options.Includes)
        {
            query = inc(query);
        }


        return query;
    }

    /// <summary>
    /// Applies filter expressions to the query to restrict results.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply filters to.</param>
    /// <param name="options">The query options containing filter expressions.</param>
    /// <returns>The queryable with filters applied.</returns>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, QueryOptions<T> options)
    {
        foreach (var filter in options.Filters)
        {
            query = query.Where(filter);
        }

        return query;
    }

    /// <summary>
    /// Applies free-text search across multiple string properties using case-insensitive contains matching.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply search to.</param>
    /// <param name="options">The query options containing search term and selectors.</param>
    /// <returns>The queryable with search applied, or the original query if no search is configured.</returns>
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, QueryOptions<T> options)
    {
        if (string.IsNullOrWhiteSpace(options.SearchTerm) || options.SearchSelectors.Count == 0)
        {
            return query;
        }

        var term = options.SearchTerm!.Trim().ToLower();
        Expression? body = null;
        var param = Expression.Parameter(typeof(T), "x");

        foreach (var selector in options.SearchSelectors)
        {
            var visitor = new ReplaceParameterVisitor(selector.Parameters[0], param);
            var selBody = visitor.Visit(selector.Body);

            var notNull = Expression.NotEqual(selBody, Expression.Constant(null, typeof(string)));
            var toLower = Expression.Call(selBody, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);
            var contains = Expression.Call(toLower, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(term));
            var safeContains = Expression.AndAlso(notNull, contains);

            body = body is null ? safeContains : Expression.OrElse(body, safeContains);
        }

        if (body is null)
        {
            return query;
        }

        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return query.Where(lambda);
    }

    /// <summary>
    /// Applies sorting to the query using the configured OrderBy function.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply sorting to.</param>
    /// <param name="options">The query options containing the ordering function.</param>
    /// <returns>The queryable with sorting applied, or the original query if no ordering is configured.</returns>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, QueryOptions<T> options)
    {
        if (options.OrderBy is not null)
        {
            return options.OrderBy(query);
        }

        return query; 
    }

    /// <summary>
    /// Applies pagination to the query using page and page size from options.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply pagination to.</param>
    /// <param name="options">The query options containing page and page size.</param>
    /// <returns>The queryable with pagination applied, or the original query if pagination is not configured.</returns>
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, QueryOptions<T> options)
    {
        if (options.Page.HasValue && options.PageSize.HasValue && options.Page > 0 && options.PageSize > 0)
        {
            var skip = (options.Page.Value - 1) * options.PageSize.Value;
            query = query.Skip(skip).Take(options.PageSize.Value);
        }
        return query;
    }

    /// <summary>
    /// Applies tracking configuration to the query based on AsNoTracking option.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply tracking configuration to.</param>
    /// <param name="options">The query options containing the tracking configuration.</param>
    /// <returns>The queryable with tracking configuration applied.</returns>
    public static IQueryable<T> ApplyTracking<T>(this IQueryable<T> query, QueryOptions<T> options) where T : class
        => options.AsNoTracking ? query.AsNoTracking() : query;

    /// <summary>
    /// Applies all query options except pagination, useful for counting total results.
    /// </summary>
    /// <typeparam name="T">The type of the queryable entity.</typeparam>
    /// <param name="query">The queryable to apply core options to.</param>
    /// <param name="options">The query options to apply.</param>
    /// <returns>The queryable with core options applied (tracking, includes, filters, search, sorting).</returns>
    public static IQueryable<T> ApplyQueryCore<T>(this IQueryable<T> query, QueryOptions<T> options) where T : class
        => query.ApplyTracking(options)
            .ApplyIncludes(options)
            .ApplyFilters(options)
            .ApplySearch(options)
            .ApplySorting(options);

    /// <summary>
    /// Expression visitor that replaces parameter expressions in lambda expressions.
    /// Used internally for building search expressions.
    /// </summary>
    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceParameterVisitor"/> class.
        /// </summary>
        /// <param name="source">The source parameter expression to replace.</param>
        /// <param name="target">The target parameter expression to use as replacement.</param>
        public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        /// <summary>
        /// Visits a parameter expression and replaces it if it matches the source parameter.
        /// </summary>
        /// <param name="node">The parameter expression to visit.</param>
        /// <returns>The target parameter expression if a match is found, otherwise the original node.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
            => node == _source ? _target : base.VisitParameter(node);
    }
}
