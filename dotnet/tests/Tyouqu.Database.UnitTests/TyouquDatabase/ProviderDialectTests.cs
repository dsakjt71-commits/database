using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.DependencyInjection;
using Tyouqu.Database.MySql;
using Tyouqu.Database.PostgreSql;
using Tyouqu.Database.Sqlite;
using Tyouqu.Database.SqlServer;

namespace Tyouqu.Database.UnitTests.TyouquDatabase;

public class ProviderDialectTests
{
    public static IEnumerable<object[]> ProviderDialects()
    {
        yield return new object[] { DatabaseProvider.SqlServer, typeof(SqlServerDialect), typeof(SqlServerConnectionFactory) };
        yield return new object[] { DatabaseProvider.MySql, typeof(MySqlDialect), typeof(MySqlConnectionFactory) };
        yield return new object[] { DatabaseProvider.PostgreSql, typeof(PostgreSqlDialect), typeof(PostgreSqlConnectionFactory) };
        yield return new object[] { DatabaseProvider.Sqlite, typeof(SqliteDialect), typeof(SqliteConnectionFactory) };
    }

    [Theory]
    [MemberData(nameof(ProviderDialects))]
    public void AddTyouquDatabase_ShouldSelectProviderImplementation(
        DatabaseProvider provider,
        Type dialectType,
        Type connectionFactoryType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTyouquDatabase(options =>
        {
            options.Provider = provider;
            options.ConnectionString = "Server=localhost;";
        });

        using var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<ISqlDialect>().Should().BeOfType(dialectType);
        serviceProvider.GetRequiredService<IDbConnectionFactory>().Should().BeOfType(connectionFactoryType);
    }

    [Fact]
    public void MySqlDialect_ShouldBuildLimitOffsetAndQuoteIdentifier()
    {
        var dialect = new MySqlDialect();

        dialect.BuildPagedSql("select * from users order by id;", 20, 10)
            .Should().Contain("limit 10 offset 20");
        dialect.QuoteIdentifier("a`b").Should().Be("`a``b`");
    }

    [Fact]
    public void PostgreSqlDialect_ShouldBuildLimitOffsetAndQuoteIdentifier()
    {
        var dialect = new PostgreSqlDialect();

        dialect.BuildPagedSql("select * from users order by id;", 20, 10)
            .Should().Contain("limit 10 offset 20");
        dialect.QuoteIdentifier("a\"b").Should().Be("\"a\"\"b\"");
    }

    [Fact]
    public void SqliteDialect_ShouldBuildLimitOffsetAndQuoteIdentifier()
    {
        var dialect = new SqliteDialect();

        dialect.BuildPagedSql("select * from users order by id;", 20, 10)
            .Should().Contain("limit 10 offset 20");
        dialect.QuoteIdentifier("a\"b").Should().Be("\"a\"\"b\"");
    }
}
