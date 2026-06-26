using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper.DependencyInjection;

namespace Tyouqu.Database.Sqlite.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquSqliteDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
        services.AddSingleton<ISqlDialect, SqliteDialect>();
        return services.AddTyouquDapperDatabaseCore();
    }
}
