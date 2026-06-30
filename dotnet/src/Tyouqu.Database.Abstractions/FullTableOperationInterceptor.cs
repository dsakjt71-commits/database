namespace Tyouqu.Database.Abstractions;

public sealed class FullTableOperationInterceptor : ISqlInterceptor
{
    private readonly SqlSafetyOptions _options;

    public FullTableOperationInterceptor(SqlSafetyOptions options)
    {
        _options = options;
    }

    public void BeforeExecute(SqlExecutionContext context)
    {
        var normalized = SqlSafetyAnalyzer.Normalize(context.Sql);
        if (_options.BlockFullTableUpdate && SqlSafetyAnalyzer.IsFullTableUpdate(normalized))
        {
            throw new TyouquDatabaseException("Unsafe SQL was blocked: UPDATE without WHERE is not allowed.")
            {
                Provider = context.Provider.ToString(),
                SqlId = context.SqlId
            };
        }

        if (_options.BlockFullTableDelete && SqlSafetyAnalyzer.IsFullTableDelete(normalized))
        {
            throw new TyouquDatabaseException("Unsafe SQL was blocked: DELETE without WHERE is not allowed.")
            {
                Provider = context.Provider.ToString(),
                SqlId = context.SqlId
            };
        }
    }
}
