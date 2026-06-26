namespace Tyouqu.Database.Abstractions;

public interface ISqlDialect
{
    string Name { get; }

    string ParameterPrefix { get; }

    string QuoteIdentifier(string identifier);

    string BuildPagedSql(string sql, int offset, int pageSize);
}

