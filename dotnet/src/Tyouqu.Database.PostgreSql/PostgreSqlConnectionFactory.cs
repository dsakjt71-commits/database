using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.PostgreSql;

public sealed class PostgreSqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public PostgreSqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new TyouquDatabaseException("Database connection string is empty.")
            {
                Provider = DatabaseProvider.PostgreSql.ToString()
            };
        }

        var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

