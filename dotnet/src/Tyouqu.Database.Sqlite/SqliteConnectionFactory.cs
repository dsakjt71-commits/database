using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Sqlite;

public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public SqliteConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new TyouquDatabaseException("Database connection string is empty.")
            {
                Provider = DatabaseProvider.Sqlite.ToString()
            };
        }

        var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

