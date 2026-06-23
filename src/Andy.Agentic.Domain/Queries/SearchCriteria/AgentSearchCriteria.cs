namespace Andy.Agentic.Domain.Queries.SearchCriteria;

/// <summary>
///     Filter criteria for querying chat history.
/// </summary>
public class ChatHistoryFilter
{
    /// <summary>
    ///     Optional agent identifier to filter by.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    ///     Optional session identifier to filter by.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    ///     Start of date range (UTC).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    ///     End of date range (UTC).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    ///     Free-text search across message content.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    ///     Sender role to filter by.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    ///     Whether to include only tool execution messages.
    /// </summary>
    public bool? IsToolExecution { get; set; }

    /// <summary>
    ///     Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    ///     Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    ///     Property name to sort by.
    /// </summary>
    public string SortBy { get; set; } = "Timestamp";

    /// <summary>
    ///     Whether to sort in descending order.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
