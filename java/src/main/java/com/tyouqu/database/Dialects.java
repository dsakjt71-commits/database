package com.tyouqu.database;

public final class Dialects {
    private Dialects() {
    }

    public static SqlDialect forProvider(DatabaseProvider provider) {
        return switch (provider) {
            case SQL_SERVER -> new SqlServerDialect();
            case MYSQL -> new MySqlDialect();
            case POSTGRESQL -> new PostgreSqlDialect();
            case SQLITE -> new SqliteDialect();
        };
    }

    private static final class SqlServerDialect implements SqlDialect {
        public String name() {
            return DatabaseProvider.SQL_SERVER.name();
        }

        public String parameterPrefix() {
            return ":";
        }

        public String quoteIdentifier(String identifier) {
            return "[" + identifier.replace("]", "]]") + "]";
        }

        public String buildPagedSql(String sql, int offset, int pageSize) {
            return trim(sql) + System.lineSeparator() + "offset " + offset + " rows fetch next " + pageSize + " rows only";
        }
    }

    private static final class MySqlDialect implements SqlDialect {
        public String name() {
            return DatabaseProvider.MYSQL.name();
        }

        public String parameterPrefix() {
            return ":";
        }

        public String quoteIdentifier(String identifier) {
            return "`" + identifier.replace("`", "``") + "`";
        }

        public String buildPagedSql(String sql, int offset, int pageSize) {
            return trim(sql) + System.lineSeparator() + "limit " + pageSize + " offset " + offset;
        }
    }

    private static final class PostgreSqlDialect implements SqlDialect {
        public String name() {
            return DatabaseProvider.POSTGRESQL.name();
        }

        public String parameterPrefix() {
            return ":";
        }

        public String quoteIdentifier(String identifier) {
            return "\"" + identifier.replace("\"", "\"\"") + "\"";
        }

        public String buildPagedSql(String sql, int offset, int pageSize) {
            return trim(sql) + System.lineSeparator() + "limit " + pageSize + " offset " + offset;
        }
    }

    private static final class SqliteDialect implements SqlDialect {
        public String name() {
            return DatabaseProvider.SQLITE.name();
        }

        public String parameterPrefix() {
            return ":";
        }

        public String quoteIdentifier(String identifier) {
            return "\"" + identifier.replace("\"", "\"\"") + "\"";
        }

        public String buildPagedSql(String sql, int offset, int pageSize) {
            return trim(sql) + System.lineSeparator() + "limit " + pageSize + " offset " + offset;
        }
    }

    private static String trim(String sql) {
        return sql.trim().replaceFirst(";\\s*$", "");
    }
}
