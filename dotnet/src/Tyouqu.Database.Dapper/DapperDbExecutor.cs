using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper;

public sealed class DapperDbExecutor : IDbExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly DatabaseOptions _options;
    private readonly ILogger<DapperDbExecutor> _logger;
    private readonly IEnumerable<ISqlInterceptor> _interceptors;
    private readonly IEnumerable<ISqlExecutionLogger> _executionLoggers;

    public DapperDbExecutor(
        IDbConnectionFactory connectionFactory,
        IOptions<DatabaseOptions> options,
        ILogger<DapperDbExecutor> logger,
        IEnumerable<ISqlInterceptor> interceptors,
        IEnumerable<ISqlExecutionLogger> executionLoggers)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
        _interceptors = interceptors;
        _executionLoggers = executionLoggers;
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(sql, parameters, SqlExecutionKind.Execute);
        BeforeExecute(context);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            var affectedRows = await connection.ExecuteAsync(command);
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, true, affectedRows, null, null, cancellationToken);
            return affectedRows;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, false, null, null, ex, cancellationToken);
            _logger.LogError(ex, "Database execute failed.");
            throw new TyouquDatabaseException("Database execute failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(sql, parameters, SqlExecutionKind.Query);
        BeforeExecute(context);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            var rows = await connection.QueryAsync<T>(command);
            var list = rows.AsList();
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, true, null, list.Count, null, cancellationToken);
            return list;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, false, null, null, ex, cancellationToken);
            _logger.LogError(ex, "Database query failed.");
            throw new TyouquDatabaseException("Database query failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(sql, parameters, SqlExecutionKind.QuerySingle);
        BeforeExecute(context);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            var row = await connection.QuerySingleOrDefaultAsync<T>(command);
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, true, null, row is null ? 0 : 1, null, cancellationToken);
            return row;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, false, null, null, ex, cancellationToken);
            _logger.LogError(ex, "Database single query failed.");
            throw new TyouquDatabaseException("Database single query failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }

    public async Task<SqlMultipleResult<TFirst, TSecond>> QueryMultipleAsync<TFirst, TSecond>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(sql, parameters, SqlExecutionKind.QueryMultiple);
        BeforeExecute(context);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            using var reader = await connection.QueryMultipleAsync(command);
            var result = new SqlMultipleResult<TFirst, TSecond>
            {
                First = (await reader.ReadAsync<TFirst>()).AsList(),
                Second = (await reader.ReadAsync<TSecond>()).AsList()
            };
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, true, null, result.First.Count + result.Second.Count, null, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, false, null, null, ex, cancellationToken);
            _logger.LogError(ex, "Database multiple query failed.");
            throw new TyouquDatabaseException("Database multiple query failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }

    public async Task<SqlMultipleResult<TFirst, TSecond, TThird>> QueryMultipleAsync<TFirst, TSecond, TThird>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(sql, parameters, SqlExecutionKind.QueryMultiple);
        BeforeExecute(context);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            using var reader = await connection.QueryMultipleAsync(command);
            var result = new SqlMultipleResult<TFirst, TSecond, TThird>
            {
                First = (await reader.ReadAsync<TFirst>()).AsList(),
                Second = (await reader.ReadAsync<TSecond>()).AsList(),
                Third = (await reader.ReadAsync<TThird>()).AsList()
            };
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, true, null, result.First.Count + result.Second.Count + result.Third.Count, null, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogAsync(context, stopwatch.ElapsedMilliseconds, false, null, null, ex, cancellationToken);
            _logger.LogError(ex, "Database multiple query failed.");
            throw new TyouquDatabaseException("Database multiple query failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }

    private SqlExecutionContext CreateContext(string sql, object? parameters, SqlExecutionKind kind)
    {
        return new SqlExecutionContext(sql, parameters, _options.Provider, kind);
    }

    private void BeforeExecute(SqlExecutionContext context)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.BeforeExecute(context);
        }
    }

    private async Task LogAsync(
        SqlExecutionContext context,
        long elapsedMilliseconds,
        bool succeeded,
        int? affectedRows,
        int? returnedRows,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (!_options.SqlLogging.Enabled || !_executionLoggers.Any())
        {
            return;
        }

        var isSlowSql = elapsedMilliseconds >= _options.SqlLogging.SlowSqlThresholdMs;
        if (_options.SqlLogging.LogOnlySlowSql && !isSlowSql)
        {
            return;
        }

        var log = new SqlExecutionLog
        {
            SqlId = context.SqlId,
            Sql = _options.SqlLogging.LogSql ? context.Sql : null,
            Parameters = _options.SqlLogging.LogParameters && _options.EnableSensitiveLogging ? context.Parameters : null,
            Provider = _options.Provider.ToString(),
            Kind = context.Kind,
            ElapsedMilliseconds = elapsedMilliseconds,
            AffectedRows = affectedRows,
            ReturnedRows = returnedRows,
            IsSlowSql = isSlowSql,
            Succeeded = succeeded,
            ErrorMessage = exception?.Message
        };

        foreach (var executionLogger in _executionLoggers)
        {
            await executionLogger.LogAsync(log, cancellationToken);
        }
    }
}
