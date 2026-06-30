namespace Tyouqu.Database.Abstractions;

public sealed class SqlExecutionLog
{
    public string? SqlId { get; init; }

    public string? Sql { get; init; }

    public object? Parameters { get; init; }

    public string Provider { get; init; } = string.Empty;

    public SqlExecutionKind Kind { get; init; }

    public long ElapsedMilliseconds { get; init; }

    public int? AffectedRows { get; init; }

    public int? ReturnedRows { get; init; }

    public bool IsSlowSql { get; init; }

    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
}
