package com.tyouqu.database;

import java.util.Map;

public record SqlExecutionContext(
    String sql,
    Map<String, ?> parameters,
    DatabaseProvider provider,
    SqlExecutionKind kind,
    String sqlId
) {
}
