package com.tyouqu.database;

public final class FullTableOperationInterceptor implements SqlInterceptor {
    private final SqlSafetyOptions options;

    public FullTableOperationInterceptor(SqlSafetyOptions options) {
        this.options = options == null ? SqlSafetyOptions.defaults() : options;
    }

    @Override
    public void beforeExecute(SqlExecutionContext context) {
        String normalized = SqlSafetyAnalyzer.normalize(context.sql());
        if (options.blockFullTableUpdate() && SqlSafetyAnalyzer.isFullTableUpdate(normalized)) {
            throw new TyouquDatabaseException("Unsafe SQL was blocked: UPDATE without WHERE is not allowed.")
                .provider(context.provider().name())
                .sqlId(context.sqlId());
        }
        if (options.blockFullTableDelete() && SqlSafetyAnalyzer.isFullTableDelete(normalized)) {
            throw new TyouquDatabaseException("Unsafe SQL was blocked: DELETE without WHERE is not allowed.")
                .provider(context.provider().name())
                .sqlId(context.sqlId());
        }
    }
}
