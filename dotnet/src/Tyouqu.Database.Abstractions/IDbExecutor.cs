namespace Tyouqu.Database.Abstractions;

public interface IDbExecutor
{
    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<SqlMultipleResult<TFirst, TSecond>> QueryMultipleAsync<TFirst, TSecond>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<SqlMultipleResult<TFirst, TSecond, TThird>> QueryMultipleAsync<TFirst, TSecond, TThird>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}
