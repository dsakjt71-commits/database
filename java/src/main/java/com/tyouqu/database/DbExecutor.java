package com.tyouqu.database;

import java.util.List;
import java.util.Map;

public interface DbExecutor {
    int execute(String sql, Map<String, ?> parameters);

    <T> List<T> query(String sql, Map<String, ?> parameters, Class<T> type);

    <T> T querySingleOrDefault(String sql, Map<String, ?> parameters, Class<T> type);
}
