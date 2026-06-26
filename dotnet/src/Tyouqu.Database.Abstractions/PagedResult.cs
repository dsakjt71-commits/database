namespace Tyouqu.Database.Abstractions;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public long TotalCount { get; init; }

    public int PageIndex { get; init; }

    public int PageSize { get; init; }
}

