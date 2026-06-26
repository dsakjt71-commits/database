namespace Tyouqu.Database.Abstractions;

public sealed class SqlTemplateOptions
{
    public string RootPath { get; set; } = "sql";

    public bool EnableHotReload { get; set; }

    public string ReloadMode { get; set; } = "Manual";

    public bool FailOnDuplicateSqlId { get; set; } = true;

    public bool FailOnMissingSqlId { get; set; } = true;
}

