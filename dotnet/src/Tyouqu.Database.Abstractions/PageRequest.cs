namespace Tyouqu.Database.Abstractions;

public sealed record PageRequest(int PageIndex, int PageSize)
{
    public int Offset => Math.Max(PageIndex - 1, 0) * PageSize;
}

