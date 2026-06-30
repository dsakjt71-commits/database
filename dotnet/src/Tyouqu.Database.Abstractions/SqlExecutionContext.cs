namespace Tyouqu.Database.Abstractions;

public sealed class SqlExecutionContext
{
    public SqlExecutionContext(
        string sql,
        object? parameters,
        DatabaseProvider provider,
        SqlExecutionKind kind,
        string? sqlId = null)
    {
        Sql = sql;
        Parameters = parameters;
        Provider = provider;
        Kind = kind;
        SqlId = sqlId;
    }

    public string Sql { get; }

    public object? Parameters { get; }

    public DatabaseProvider Provider { get; }

    public SqlExecutionKind Kind { get; }

    public string? SqlId { get; }
}
