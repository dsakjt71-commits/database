using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.SqlServer.DependencyInjection;

var connectionString = Environment.GetEnvironmentVariable("TYOUQU_DB_CONNECTION");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Please set TYOUQU_DB_CONNECTION before running this demo.");
    Console.WriteLine("Example:");
    Console.WriteLine("  $env:TYOUQU_DB_CONNECTION='Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;'");
    return 2;
}

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
}).SetMinimumLevel(LogLevel.Information));

services.AddTyouquSqlServerDatabase(options =>
{
    options.Provider = DatabaseProvider.SqlServer;
    options.ConnectionString = connectionString;
    options.CommandTimeoutSeconds = 30;
    options.SqlTemplates.RootPath = "sql";
    options.SqlTemplates.ReloadMode = "Manual";
});

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<ISqlTemplateExecutor>();

var appCount = await db.QuerySingleOrDefaultByIdAsync<int>("demo.app.count");
var apps = await db.QueryByIdAsync<AppRow>("demo.app.list", new { Take = 5 });
var paged = await db.QueryPagedByIdAsync<AppRow>("demo.app.page", null, new PageRequest(1, 3));
var multiple = await db.QueryMultipleByIdAsync<AppRow, AppStats>(
    "demo.app.detailWithStats",
    new { AppId = 1 });

Console.WriteLine($"App count: {appCount}");
Console.WriteLine("Latest apps:");

foreach (var app in apps)
{
    Console.WriteLine($"  {app.AppId} | {app.AppName} | {app.ClientId} | {app.Status}");
}

Console.WriteLine($"Paged apps: page={paged.PageIndex}, size={paged.PageSize}, total={paged.TotalCount}, items={paged.Items.Count}");
Console.WriteLine($"Multiple result sets: app={multiple.First.FirstOrDefault()?.ClientId}, total={multiple.Second.FirstOrDefault()?.TotalApps}");

return 0;

public sealed class AppRow
{
    public long AppId { get; set; }

    public string AppName { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public int Status { get; set; }
}

public sealed class AppStats
{
    public int TotalApps { get; set; }

    public int EnabledApps { get; set; }
}
