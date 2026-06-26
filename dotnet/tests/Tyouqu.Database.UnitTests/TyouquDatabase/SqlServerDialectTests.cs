using FluentAssertions;
using Tyouqu.Database.SqlServer;

namespace Tyouqu.Database.UnitTests.TyouquDatabase;

public class SqlServerDialectTests
{
    [Fact]
    public void BuildPagedSql_ShouldAppendOffsetFetch()
    {
        var dialect = new SqlServerDialect();

        var sql = dialect.BuildPagedSql("select * from Apps order by app_id desc;", 10, 5);

        sql.Should().Contain("offset 10 rows fetch next 5 rows only");
        sql.Should().StartWith("select * from Apps order by app_id desc");
    }

    [Fact]
    public void QuoteIdentifier_ShouldEscapeClosingBracket()
    {
        var dialect = new SqlServerDialect();

        dialect.QuoteIdentifier("a]b").Should().Be("[a]]b]");
    }
}

