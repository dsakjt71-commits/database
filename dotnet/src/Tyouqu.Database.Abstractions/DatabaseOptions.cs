namespace Tyouqu.Database.Abstractions;

public sealed class DatabaseOptions
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    public string ConnectionString { get; set; } = string.Empty;

    public int CommandTimeoutSeconds { get; set; } = 30;

    public bool EnableSensitiveLogging { get; set; }

    public SqlTemplateOptions SqlTemplates { get; set; } = new();

    public SqlExecutionLogOptions SqlLogging { get; set; } = new();

    public SqlSafetyOptions Safety { get; set; } = new();
}

public sealed class SqlExecutionLogOptions
{
    public bool Enabled { get; set; }

    public bool LogSql { get; set; } = true;

    public bool LogParameters { get; set; }

    public bool LogOnlySlowSql { get; set; }

    public int SlowSqlThresholdMs { get; set; } = 500;
}

public sealed class SqlSafetyOptions
{
    public bool BlockFullTableUpdate { get; set; } = true;

    public bool BlockFullTableDelete { get; set; } = true;
}

