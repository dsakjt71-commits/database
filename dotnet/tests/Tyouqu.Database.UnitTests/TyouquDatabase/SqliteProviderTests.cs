using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Sqlite.DependencyInjection;

namespace Tyouqu.Database.UnitTests.TyouquDatabase;

public class SqliteProviderTests : IDisposable
{
    private readonly string _rootPath;
    private readonly string _dbPath;
    private ServiceProvider? _serviceProvider;

    public SqliteProviderTests()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), "tyouqu-sqlite-tests", Guid.NewGuid().ToString("N"));
        _rootPath = Path.Combine(testRoot, "sql");
        _dbPath = Path.Combine(testRoot, "demo.db");
        Directory.CreateDirectory(Path.Combine(_rootPath, "sqlite"));
    }

    [Fact]
    public async Task SqliteProvider_ShouldExecuteQueryPageAndMultipleResultSets()
    {
        WriteSql("demo.schema.sql", """
            create table apps (
                app_id integer primary key,
                app_name text not null,
                status integer not null
            );

            insert into apps(app_id, app_name, status) values (1, 'App A', 1);
            insert into apps(app_id, app_name, status) values (2, 'App B', 0);
            insert into apps(app_id, app_name, status) values (3, 'App C', 1);
            """);
        WriteSql("demo.getById.sql", """
            select
                app_id as AppId,
                app_name as AppName,
                status as Status
            from apps
            where app_id = @AppId;
            """);
        WriteSql("demo.list.sql", """
            select
                app_id as AppId,
                app_name as AppName,
                status as Status
            from apps
            order by app_id;
            """);
        WriteSql("demo.multi.sql", """
            select
                app_id as AppId,
                app_name as AppName,
                status as Status
            from apps
            where app_id = @AppId;

            select
                count(1) as Total,
                sum(case when status = 1 then 1 else 0 end) as Enabled
            from apps;
            """);

        var db = CreateExecutor();

        await db.ExecuteByIdAsync("demo.schema");
        var app = await db.QuerySingleOrDefaultByIdAsync<AppRow>("demo.getById", new { AppId = 1 });
        var page = await db.QueryPagedByIdAsync<AppRow>("demo.list", null, new PageRequest(1, 2));
        var multi = await db.QueryMultipleByIdAsync<AppRow, AppStats>("demo.multi", new { AppId = 2 });

        app.Should().NotBeNull();
        app!.AppName.Should().Be("App A");
        page.TotalCount.Should().Be(3);
        page.Items.Should().HaveCount(2);
        multi.First.Should().ContainSingle(x => x.AppName == "App B");
        multi.Second.Should().ContainSingle();
        multi.Second[0].Total.Should().Be(3);
        multi.Second[0].Enabled.Should().Be(2);
    }

    [Fact]
    public async Task SqliteProvider_ShouldBlockFullTableDelete()
    {
        WriteSql("demo.schema.sql", """
            create table apps (
                app_id integer primary key,
                app_name text not null
            );

            insert into apps(app_id, app_name) values (1, 'App A');
            """);
        WriteSql("demo.deleteAll.sql", "delete from apps");

        var db = CreateExecutor();

        await db.ExecuteByIdAsync("demo.schema");
        var act = () => db.ExecuteByIdAsync("demo.deleteAll");

        await act.Should().ThrowAsync<TyouquDatabaseException>()
            .WithMessage("*DELETE without WHERE*");
    }

    [Fact]
    public async Task SqliteProvider_ShouldWriteSqlExecutionLogWhenEnabled()
    {
        var executionLogger = new CaptureSqlExecutionLogger();
        WriteSql("demo.schema.sql", """
            create table apps (
                app_id integer primary key,
                app_name text not null
            );
            """);

        var db = CreateExecutor(options =>
        {
            options.SqlLogging.Enabled = true;
            options.SqlLogging.LogSql = true;
        }, services => services.AddSingleton<ISqlExecutionLogger>(executionLogger));

        await db.ExecuteByIdAsync("demo.schema");

        executionLogger.Logs.Should().ContainSingle();
        executionLogger.Logs[0].Sql.Should().Contain("create table apps");
        executionLogger.Logs[0].ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(0);
        executionLogger.Logs[0].Succeeded.Should().BeTrue();
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        SqliteConnection.ClearAllPools();

        var testRoot = Directory.GetParent(_rootPath)!.FullName;
        if (Directory.Exists(testRoot))
            Directory.Delete(testRoot, recursive: true);
    }

    private ISqlTemplateExecutor CreateExecutor(
        Action<DatabaseOptions>? configure = null,
        Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        configureServices?.Invoke(services);
        services.AddTyouquSqliteDatabase(options =>
        {
            options.Provider = DatabaseProvider.Sqlite;
            options.ConnectionString = $"Data Source={_dbPath}";
            options.SqlTemplates.RootPath = _rootPath;
            configure?.Invoke(options);
        });

        _serviceProvider = services.BuildServiceProvider();
        return _serviceProvider.GetRequiredService<ISqlTemplateExecutor>();
    }

    private void WriteSql(string fileName, string sql)
    {
        File.WriteAllText(Path.Combine(_rootPath, "sqlite", fileName), sql);
    }

    public sealed class AppRow
    {
        public int AppId { get; set; }

        public string AppName { get; set; } = string.Empty;

        public int Status { get; set; }
    }

    public sealed class AppStats
    {
        public int Total { get; set; }

        public int Enabled { get; set; }
    }

    private sealed class CaptureSqlExecutionLogger : ISqlExecutionLogger
    {
        public List<SqlExecutionLog> Logs { get; } = [];

        public Task LogAsync(SqlExecutionLog log, CancellationToken cancellationToken = default)
        {
            Logs.Add(log);
            return Task.CompletedTask;
        }
    }
}
