using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.Dapper.DependencyInjection;

namespace Tyouqu.Database.SqlServer.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquSqlServerDatabase(
        this IServiceCollection services,
        Action<DatabaseOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionFactory, SqlServerConnectionFactory>();
        services.AddSingleton<ISqlDialect, SqlServerDialect>();
        return services.AddTyouquDapperDatabaseCore();
    }
}
