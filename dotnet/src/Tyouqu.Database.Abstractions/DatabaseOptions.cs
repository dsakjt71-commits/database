namespace Tyouqu.Database.Abstractions;

public sealed class DatabaseOptions
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

    public string ConnectionString { get; set; } = string.Empty;

    public int CommandTimeoutSeconds { get; set; } = 30;

    public bool EnableSensitiveLogging { get; set; }

    public SqlTemplateOptions SqlTemplates { get; set; } = new();
}

