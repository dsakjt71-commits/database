using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.MySql;

public sealed class MySqlDialect : ISqlDialect
{
    public string Name => DatabaseProvider.MySql.ToString();

    public string ParameterPrefix => "@";

    public string QuoteIdentifier(string identifier)
    {
        return $"`{identifier.Replace("`", "``")}`";
    }

    public string BuildPagedSql(string sql, int offset, int pageSize)
    {
        var normalized = sql.Trim().TrimEnd(';');
        return $"""
               {normalized}
               limit {pageSize} offset {offset}
               """;
    }
}

