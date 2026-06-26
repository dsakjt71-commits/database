package com.tyouqu.database;

import java.util.List;
import java.util.Map;

public final class SqlTemplateExecutor {
    private final DbExecutor db;
    private final SqlTemplateStore sqlStore;
    private final SqlDialect dialect;

    public SqlTemplateExecutor(DbExecutor db, SqlTemplateStore sqlStore, SqlDialect dialect) {
        this.db = db;
        this.sqlStore = sqlStore;
        this.dialect = dialect;
    }

    public int executeById(String sqlId, Map<String, ?> parameters) {
        try {
            return db.execute(sqlStore.getRequiredSql(sqlId), parameters);
        } catch (TyouquDatabaseException ex) {
            if (ex.sqlId() == null) {
                ex.sqlId(sqlId);
            }
            throw ex;
        }
    }

    public <T> List<T> queryById(String sqlId, Map<String, ?> parameters, Class<T> type) {
        try {
            return db.query(sqlStore.getRequiredSql(sqlId), parameters, type);
        } catch (TyouquDatabaseException ex) {
            if (ex.sqlId() == null) {
                ex.sqlId(sqlId);
            }
            throw ex;
        }
    }

    public <T> T querySingleOrDefaultById(String sqlId, Map<String, ?> parameters, Class<T> type) {
        try {
            return db.querySingleOrDefault(sqlStore.getRequiredSql(sqlId), parameters, type);
        } catch (TyouquDatabaseException ex) {
            if (ex.sqlId() == null) {
                ex.sqlId(sqlId);
            }
            throw ex;
        }
    }

    public <T> PagedResult<T> queryPagedById(String sqlId, Map<String, ?> parameters, PageRequest page, Class<T> type) {
        String sql = sqlStore.getRequiredSql(sqlId);
        String countSql = "select count(1) from (" + removeTrailingOrderBy(trimTrailingSemicolon(sql)) + ") as _paged_source";
        String pagedSql = dialect.buildPagedSql(sql, page.offset(), page.pageSize());
        Long total = db.querySingleOrDefault(countSql, parameters, Long.class);
        List<T> items = db.query(pagedSql, parameters, type);
        return new PagedResult<>(items, total == null ? 0 : total, page.pageIndex(), page.pageSize());
    }

    private static String trimTrailingSemicolon(String sql) {
        return sql.trim().replaceFirst(";\\s*$", "");
    }

    private static String removeTrailingOrderBy(String sql) {
        String lower = sql.toLowerCase();
        int index = lower.lastIndexOf("order by");
        return index < 0 ? sql : sql.substring(0, index).stripTrailing();
    }
}
