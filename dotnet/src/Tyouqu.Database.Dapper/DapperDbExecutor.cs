using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper;

public sealed class DapperDbExecutor : IDbExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly DatabaseOptions _options;
    private readonly ILogger<DapperDbExecutor> _logger;

    public DapperDbExecutor(
        IDbConnectionFactory connectionFactory,
        IOptions<DatabaseOptions> options,
        ILogger<DapperDbExecutor> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            return await connection.ExecuteAsync(command);
        }
        catch (Exception ex)
        {
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
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            var rows = await connection.QueryAsync<T>(command);
            return rows.AsList();
        }
        catch (Exception ex)
        {
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
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            return await connection.QuerySingleOrDefaultAsync<T>(command);
        }
        catch (Exception ex)
        {
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
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            using var reader = await connection.QueryMultipleAsync(command);
            return new SqlMultipleResult<TFirst, TSecond>
            {
                First = (await reader.ReadAsync<TFirst>()).AsList(),
                Second = (await reader.ReadAsync<TSecond>()).AsList()
            };
        }
        catch (Exception ex)
        {
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
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
            var command = new CommandDefinition(
                sql,
                parameters,
                commandTimeout: _options.CommandTimeoutSeconds,
                cancellationToken: cancellationToken);
            using var reader = await connection.QueryMultipleAsync(command);
            return new SqlMultipleResult<TFirst, TSecond, TThird>
            {
                First = (await reader.ReadAsync<TFirst>()).AsList(),
                Second = (await reader.ReadAsync<TSecond>()).AsList(),
                Third = (await reader.ReadAsync<TThird>()).AsList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database multiple query failed.");
            throw new TyouquDatabaseException("Database multiple query failed.", ex)
            {
                Provider = _options.Provider.ToString()
            };
        }
    }
}
