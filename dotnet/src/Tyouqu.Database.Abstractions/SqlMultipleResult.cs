namespace Tyouqu.Database.Abstractions;

public sealed class SqlMultipleResult<TFirst, TSecond>
{
    public IReadOnlyList<TFirst> First { get; init; } = Array.Empty<TFirst>();

    public IReadOnlyList<TSecond> Second { get; init; } = Array.Empty<TSecond>();
}

public sealed class SqlMultipleResult<TFirst, TSecond, TThird>
{
    public IReadOnlyList<TFirst> First { get; init; } = Array.Empty<TFirst>();

    public IReadOnlyList<TSecond> Second { get; init; } = Array.Empty<TSecond>();

    public IReadOnlyList<TThird> Third { get; init; } = Array.Empty<TThird>();
}

