using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.MySql;

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public MySqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new TyouquDatabaseException("Database connection string is empty.")
            {
                Provider = DatabaseProvider.MySql.ToString()
            };
        }

        var connection = new MySqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
