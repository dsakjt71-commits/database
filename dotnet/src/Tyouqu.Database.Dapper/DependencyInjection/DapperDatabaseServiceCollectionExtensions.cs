using Microsoft.Extensions.DependencyInjection;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper.DependencyInjection;

public static class DapperDatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddTyouquDapperDatabaseCore(this IServiceCollection services)
    {
        services.AddSingleton<ISqlTemplateStore, FileSqlTemplateStore>();
        services.AddSingleton<ISqlInterceptor>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
            return new FullTableOperationInterceptor(options.Safety);
        });
        services.AddSingleton<ISqlExecutionLogger, LoggingSqlExecutionLogger>();
        services.AddScoped<IDbExecutor, DapperDbExecutor>();
        services.AddScoped<ISqlTemplateExecutor, DapperSqlTemplateExecutor>();
        return services;
    }
}
