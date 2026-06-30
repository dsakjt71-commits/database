package com.tyouqu.database;

public interface SqlInterceptor {
    void beforeExecute(SqlExecutionContext context);
}
