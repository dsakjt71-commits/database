using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.SqlServer;

public sealed class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public SqlServerConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new TyouquDatabaseException("Database connection string is empty.")
            {
                Provider = DatabaseProvider.SqlServer.ToString()
            };
        }

        var connection = new SqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

