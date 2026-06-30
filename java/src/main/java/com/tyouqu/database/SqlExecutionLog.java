package com.tyouqu.database;

import java.time.OffsetDateTime;
import java.util.Map;

public record SqlExecutionLog(
    String sqlId,
    String sql,
    Map<String, ?> parameters,
    DatabaseProvider provider,
    SqlExecutionKind kind,
    long elapsedMilliseconds,
    Integer affectedRows,
    Integer returnedRows,
    boolean slowSql,
    boolean succeeded,
    String errorMessage,
    OffsetDateTime executedAt
) {
}
