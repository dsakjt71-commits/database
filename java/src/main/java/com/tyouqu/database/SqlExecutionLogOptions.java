package com.tyouqu.database;

public record SqlExecutionLogOptions(
    boolean enabled,
    boolean logSql,
    boolean logParameters,
    boolean logOnlySlowSql,
    long slowSqlThresholdMs
) {
    public SqlExecutionLogOptions {
        slowSqlThresholdMs = slowSqlThresholdMs <= 0 ? 500 : slowSqlThresholdMs;
    }

    public static SqlExecutionLogOptions disabled() {
        return new SqlExecutionLogOptions(false, true, false, false, 500);
    }
}
