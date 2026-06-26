using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper.DependencyInjection;
using Tyouqu.Database.MySql;
using Tyouqu.Database.PostgreSql;
using Tyouqu.Database.Sqlite;
using Tyouqu.Database.SqlServer;

namespace Tyouqu.Database.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionFactory>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return options.Provider switch
            {
                DatabaseProvider.SqlServer => ActivatorUtilities.CreateInstance<SqlServerConnectionFactory>(provider),
                DatabaseProvider.MySql => ActivatorUtilities.CreateInstance<MySqlConnectionFactory>(provider),
                DatabaseProvider.PostgreSql => ActivatorUtilities.CreateInstance<PostgreSqlConnectionFactory>(provider),
                DatabaseProvider.Sqlite => ActivatorUtilities.CreateInstance<SqliteConnectionFactory>(provider),
                _ => throw new TyouquDatabaseException($"Unsupported database provider. Provider={options.Provider}")
                {
                    Provider = options.Provider.ToString()
                }
            };
        });
        services.AddSingleton<ISqlDialect>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            return options.Provider switch
            {
                DatabaseProvider.SqlServer => new SqlServerDialect(),
                DatabaseProvider.MySql => new MySqlDialect(),
                DatabaseProvider.PostgreSql => new PostgreSqlDialect(),
                DatabaseProvider.Sqlite => new SqliteDialect(),
                _ => throw new TyouquDatabaseException($"Unsupported database provider. Provider={options.Provider}")
                {
                    Provider = options.Provider.ToString()
                }
            };
        });
        services.AddTyouquDapperDatabaseCore();
        return services;
    }
}
