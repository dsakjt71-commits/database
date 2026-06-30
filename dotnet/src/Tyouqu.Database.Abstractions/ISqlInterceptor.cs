namespace Tyouqu.Database.Abstractions;

public interface ISqlInterceptor
{
    void BeforeExecute(SqlExecutionContext context);
}
