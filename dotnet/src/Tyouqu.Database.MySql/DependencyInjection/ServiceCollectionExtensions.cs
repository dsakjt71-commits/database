using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper.DependencyInjection;

namespace Tyouqu.Database.MySql.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquMySqlDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
        services.AddSingleton<ISqlDialect, MySqlDialect>();
        return services.AddTyouquDapperDatabaseCore();
    }
}
