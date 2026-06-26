using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.SqlServer;

public sealed class SqlServerDialect : ISqlDialect
{
    public string Name => DatabaseProvider.SqlServer.ToString();

    public string ParameterPrefix => "@";

    public string QuoteIdentifier(string identifier)
    {
        return $"[{identifier.Replace("]", "]]")}]";
    }

    public string BuildPagedSql(string sql, int offset, int pageSize)
    {
        var normalized = sql.Trim().TrimEnd(';');
        return $"""
               {normalized}
               offset {offset} rows fetch next {pageSize} rows only
               """;
    }
}

