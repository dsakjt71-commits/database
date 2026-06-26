using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper;

namespace Tyouqu.Database.UnitTests.TyouquDatabase;

public class FileSqlTemplateStoreTests : IDisposable
{
    private readonly string _rootPath;

    public FileSqlTemplateStoreTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "tyouqu-sql-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_rootPath);
    }

    [Fact]
    public void Constructor_ShouldLoadCommonAndProviderTemplates()
    {
        WriteSql("common/demo.common.sql", "select 1;");
        WriteSql("sqlserver/demo.provider.sql", "select 2;");

        var store = CreateStore();

        store.GetRequiredSql("demo.common").Should().Be("select 1;");
        store.GetRequiredSql("demo.provider").Should().Be("select 2;");
    }

    [Fact]
    public void Constructor_ShouldLetProviderOverrideCommonTemplate()
    {
        WriteSql("common/demo.same.sql", "select 1;");
        WriteSql("sqlserver/demo.same.sql", "select 2;");

        var store = CreateStore();

        store.GetRequiredSql("demo.same").Should().Be("select 2;");
    }

    [Fact]
    public void GetRequiredSql_ShouldThrow_WhenSqlIdDoesNotExist()
    {
        WriteSql("sqlserver/demo.exists.sql", "select 1;");
        var store = CreateStore();

        var act = () => store.GetRequiredSql("demo.missing");

        act.Should().Throw<TyouquDatabaseException>()
            .WithMessage("*demo.missing*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileContainsLegacyIdMarker()
    {
        WriteSql("sqlserver/demo.legacy.sql", """
            -- @id demo.legacy
            select 1;
            """);

        var act = CreateStore;

        act.Should().Throw<TyouquDatabaseException>()
            .WithMessage("*Remove -- @id marker*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSqlFileIsEmpty()
    {
        WriteSql("sqlserver/demo.empty.sql", "   ");

        var act = CreateStore;

        act.Should().Throw<TyouquDatabaseException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDuplicateSqlIdExistsInSameScope()
    {
        WriteSql("sqlserver/a/demo.duplicate.sql", "select 1;");
        WriteSql("sqlserver/b/demo.duplicate.sql", "select 2;");

        var act = CreateStore;

        act.Should().Throw<TyouquDatabaseException>()
            .WithMessage("*Duplicate SQL template id*");
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, recursive: true);
    }

    private FileSqlTemplateStore CreateStore()
    {
        var options = Options.Create(new DatabaseOptions
        {
            Provider = DatabaseProvider.SqlServer,
            SqlTemplates = new SqlTemplateOptions
            {
                RootPath = _rootPath
            }
        });

        return new FileSqlTemplateStore(options, NullLogger<FileSqlTemplateStore>.Instance);
    }

    private void WriteSql(string relativePath, string content)
    {
        var path = Path.Combine(_rootPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }
}
