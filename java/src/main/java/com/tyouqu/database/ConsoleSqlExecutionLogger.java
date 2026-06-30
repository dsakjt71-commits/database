package com.tyouqu.database;

public final class ConsoleSqlExecutionLogger implements SqlExecutionLogger {
    @Override
    public void log(SqlExecutionLog log) {
        System.out.println(
            "SQL " + (log.succeeded() ? "executed" : "failed")
                + ". provider=" + log.provider()
                + ", kind=" + log.kind()
                + ", elapsedMs=" + log.elapsedMilliseconds()
                + ", slowSql=" + log.slowSql()
                + ", affectedRows=" + log.affectedRows()
                + ", returnedRows=" + log.returnedRows()
                + ", error=" + log.errorMessage()
                + ", sql=" + log.sql()
                + ", parameters=" + log.parameters()
        );
    }
}
