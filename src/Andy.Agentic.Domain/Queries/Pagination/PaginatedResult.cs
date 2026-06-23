namespace Andy.Agentic.Domain.Queries.Pagination;

/// <summary>
///     Represents a paginated result set with metadata about the current page, total count, and navigation.
///     Provides convenient properties for building pagination UI controls and navigation.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PaginatedResult{T}" /> class.
    /// </summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="page">The current page number (1-based indexing).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page <= 0 ? 1 : page;
        PageSize = pageSize <= 0 ? totalCount : pageSize;
    }

    /// <summary>
    ///     Gets the collection of items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    ///     Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    ///     Gets the current page number (1-based indexing).
    /// </summary>
    public int Page { get; }

    /// <summary>
    ///     Gets the number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     Gets the total number of pages based on the total count and page size.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

    /// <summary>
    ///     Gets a value indicating whether there is a previous page available.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    ///     Gets a value indicating whether there is a next page available.
    /// </summary>
    public bool HasNext => Page < TotalPages;
}
