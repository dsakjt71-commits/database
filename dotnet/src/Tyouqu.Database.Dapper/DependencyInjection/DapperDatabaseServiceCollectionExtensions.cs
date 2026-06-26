using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper.DependencyInjection;

public static class DapperDatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquDapperDatabaseCore(this IServiceCollection services)
    {
        services.AddSingleton<ISqlTemplateStore, FileSqlTemplateStore>();
        services.AddScoped<IDbExecutor, DapperDbExecutor>();
        services.AddScoped<ISqlTemplateExecutor, DapperSqlTemplateExecutor>();
        return services;
    }
}
