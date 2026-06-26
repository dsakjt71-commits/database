namespace Tyouqu.Database.Abstractions;

public interface ISqlTemplateExecutor
{
    Task<int> ExecuteByIdAsync(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryByIdAsync<T>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultByIdAsync<T>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<T>> QueryPagedByIdAsync<T>(
        string sqlId,
        object? parameters,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<SqlMultipleResult<TFirst, TSecond>> QueryMultipleByIdAsync<TFirst, TSecond>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default);

    Task<SqlMultipleResult<TFirst, TSecond, TThird>> QueryMultipleByIdAsync<TFirst, TSecond, TThird>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default);
}
