using Microsoft.Extensions.Logging;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper;

public sealed class DapperSqlTemplateExecutor : ISqlTemplateExecutor
{
    private readonly IDbExecutor _db;
    private readonly ISqlTemplateStore _sqlStore;
    private readonly ISqlDialect _dialect;
    private readonly ILogger<DapperSqlTemplateExecutor> _logger;

    public DapperSqlTemplateExecutor(
        IDbExecutor db,
        ISqlTemplateStore sqlStore,
        ISqlDialect dialect,
        ILogger<DapperSqlTemplateExecutor> logger)
    {
        _db = db;
        _sqlStore = sqlStore;
        _dialect = dialect;
        _logger = logger;
    }

    public async Task<int> ExecuteByIdAsync(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _sqlStore.GetRequiredSql(sqlId);
        try
        {
            return await _db.ExecuteAsync(sql, parameters, cancellationToken);
        }
        catch (TyouquDatabaseException ex)
        {
            ex.SqlId ??= sqlId;
            throw;
        }
    }

    public async Task<IReadOnlyList<T>> QueryByIdAsync<T>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _sqlStore.GetRequiredSql(sqlId);
        try
        {
            return await _db.QueryAsync<T>(sql, parameters, cancellationToken);
        }
        catch (TyouquDatabaseException ex)
        {
            ex.SqlId ??= sqlId;
            throw;
        }
    }

    public async Task<T?> QuerySingleOrDefaultByIdAsync<T>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _sqlStore.GetRequiredSql(sqlId);
        try
        {
            return await _db.QuerySingleOrDefaultAsync<T>(sql, parameters, cancellationToken);
        }
        catch (TyouquDatabaseException ex)
        {
            ex.SqlId ??= sqlId;
            throw;
        }
    }

    public async Task<PagedResult<T>> QueryPagedByIdAsync<T>(
        string sqlId,
        object? parameters,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        if (page.PageIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "PageIndex must be greater than or equal to 1.");
        }

        if (page.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "PageSize must be greater than or equal to 1.");
        }

        var sql = _sqlStore.GetRequiredSql(sqlId);
        var countSql = $"select count(1) from ({RemoveTrailingOrderBy(TrimTrailingSemicolon(sql))}) as _paged_source";
        var pagedSql = _dialect.BuildPagedSql(sql, page.Offset, page.PageSize);

        _logger.LogDebug("Executing paged sql. SqlId={SqlId}, PageIndex={PageIndex}, PageSize={PageSize}", sqlId, page.PageIndex, page.PageSize);

        var total = await _db.QuerySingleOrDefaultAsync<long>(countSql, parameters, cancellationToken);
        var items = await _db.QueryAsync<T>(pagedSql, parameters, cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            PageIndex = page.PageIndex,
            PageSize = page.PageSize
        };
    }

    public async Task<SqlMultipleResult<TFirst, TSecond>> QueryMultipleByIdAsync<TFirst, TSecond>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _sqlStore.GetRequiredSql(sqlId);
        try
        {
            return await _db.QueryMultipleAsync<TFirst, TSecond>(sql, parameters, cancellationToken);
        }
        catch (TyouquDatabaseException ex)
        {
            ex.SqlId ??= sqlId;
            throw;
        }
    }

    public async Task<SqlMultipleResult<TFirst, TSecond, TThird>> QueryMultipleByIdAsync<TFirst, TSecond, TThird>(
        string sqlId,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var sql = _sqlStore.GetRequiredSql(sqlId);
        try
        {
            return await _db.QueryMultipleAsync<TFirst, TSecond, TThird>(sql, parameters, cancellationToken);
        }
        catch (TyouquDatabaseException ex)
        {
            ex.SqlId ??= sqlId;
            throw;
        }
    }

    private static string TrimTrailingSemicolon(string sql)
    {
        return sql.Trim().TrimEnd(';');
    }

    private static string RemoveTrailingOrderBy(string sql)
    {
        var index = sql.LastIndexOf("order by", StringComparison.OrdinalIgnoreCase);
        return index < 0 ? sql : sql[..index].TrimEnd();
    }
}
