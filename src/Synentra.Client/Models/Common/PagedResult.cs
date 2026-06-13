namespace Vectra.Client.Models.Common;

/// <summary>
/// Represents a paginated list of items returned by the Vectra API.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Gets the items in the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Gets the current page number (1-based).</summary>
    public int Page { get; init; }

    /// <summary>Gets the maximum number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of items across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets whether there is a next page available.</summary>
    public bool HasNextPage => Page * PageSize < TotalCount;

    /// <summary>Gets whether there is a previous page available.</summary>
    public bool HasPreviousPage => Page > 1;
}
