package com.tyouqu.database;

public interface SqlDialect {
    String name();

    String parameterPrefix();

    String quoteIdentifier(String identifier);

    String buildPagedSql(String sql, int offset, int pageSize);
}
