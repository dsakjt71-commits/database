namespace Tyouqu.Database.Abstractions;

public interface ISqlExecutionLogger
{
    Task LogAsync(SqlExecutionLog log, CancellationToken cancellationToken = default);
}
