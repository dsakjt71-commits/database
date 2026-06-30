using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper;

public sealed class LoggingSqlExecutionLogger : ISqlExecutionLogger
{
    private readonly ILogger<LoggingSqlExecutionLogger> _logger;
    private readonly DatabaseOptions _options;

    public LoggingSqlExecutionLogger(
        ILogger<LoggingSqlExecutionLogger> logger,
        IOptions<DatabaseOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task LogAsync(SqlExecutionLog log, CancellationToken cancellationToken = default)
    {
        if (!_options.SqlLogging.Enabled)
        {
            return Task.CompletedTask;
        }

        if (log.Succeeded)
        {
            _logger.LogInformation(
                "SQL executed. Provider={Provider}, Kind={Kind}, ElapsedMs={ElapsedMs}, IsSlowSql={IsSlowSql}, AffectedRows={AffectedRows}, ReturnedRows={ReturnedRows}, Sql={Sql}, Parameters={Parameters}",
                log.Provider,
                log.Kind,
                log.ElapsedMilliseconds,
                log.IsSlowSql,
                log.AffectedRows,
                log.ReturnedRows,
                log.Sql,
                log.Parameters);
            return Task.CompletedTask;
        }

        _logger.LogWarning(
            "SQL failed. Provider={Provider}, Kind={Kind}, ElapsedMs={ElapsedMs}, IsSlowSql={IsSlowSql}, Error={Error}, Sql={Sql}, Parameters={Parameters}",
            log.Provider,
            log.Kind,
            log.ElapsedMilliseconds,
            log.IsSlowSql,
            log.ErrorMessage,
            log.Sql,
            log.Parameters);
        return Task.CompletedTask;
    }
}
