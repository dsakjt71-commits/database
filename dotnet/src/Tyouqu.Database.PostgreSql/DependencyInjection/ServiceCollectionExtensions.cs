using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper.DependencyInjection;

namespace Tyouqu.Database.PostgreSql.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquPostgreSqlDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionFactory, PostgreSqlConnectionFactory>();
        services.AddSingleton<ISqlDialect, PostgreSqlDialect>();
        return services.AddTyouquDapperDatabaseCore();
    }
}
