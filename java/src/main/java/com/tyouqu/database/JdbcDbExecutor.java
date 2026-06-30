package com.tyouqu.database;

import javax.sql.DataSource;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.Map;

public final class JdbcDbExecutor implements DbExecutor {
    private final DataSource dataSource;
    private final DatabaseOptions options;
    private final List<SqlInterceptor> interceptors;
    private final List<SqlExecutionLogger> executionLoggers;

    public JdbcDbExecutor(DataSource dataSource) {
        this(dataSource, DatabaseOptions.builder().build(), List.of(new FullTableOperationInterceptor(SqlSafetyOptions.defaults())), List.of());
    }

    public JdbcDbExecutor(DataSource dataSource, int commandTimeoutSeconds) {
        this(dataSource, DatabaseOptions.builder().commandTimeoutSeconds(commandTimeoutSeconds).build());
    }

    public JdbcDbExecutor(DataSource dataSource, DatabaseOptions options) {
        this(dataSource, options, defaultInterceptors(options), List.of());
    }

    public JdbcDbExecutor(
        DataSource dataSource,
        DatabaseOptions options,
        List<SqlInterceptor> interceptors,
        List<SqlExecutionLogger> executionLoggers
    ) {
        this.dataSource = dataSource;
        this.options = options == null ? DatabaseOptions.builder().build() : options;
        this.interceptors = interceptors == null ? List.of() : List.copyOf(interceptors);
        this.executionLoggers = executionLoggers == null ? List.of() : List.copyOf(executionLoggers);
    }

    public int execute(String sql, Map<String, ?> parameters) {
        SqlExecutionContext context = createContext(sql, parameters, SqlExecutionKind.EXECUTE);
        beforeExecute(context);
        NamedParameterSql.ParsedSql parsedSql = NamedParameterSql.parse(sql);
        long start = System.nanoTime();
        try (Connection connection = dataSource.getConnection();
             PreparedStatement statement = connection.prepareStatement(parsedSql.jdbcSql())) {
            bind(statement, parsedSql.parameterNames(), parameters);
            int affectedRows = statement.executeUpdate();
            log(context, elapsedMilliseconds(start), true, affectedRows, null, null);
            return affectedRows;
        } catch (SQLException ex) {
            log(context, elapsedMilliseconds(start), false, null, null, ex);
            throw new TyouquDatabaseException("Database execute failed.", ex);
        }
    }

    public <T> List<T> query(String sql, Map<String, ?> parameters, Class<T> type) {
        SqlExecutionContext context = createContext(sql, parameters, SqlExecutionKind.QUERY);
        beforeExecute(context);
        NamedParameterSql.ParsedSql parsedSql = NamedParameterSql.parse(sql);
        long start = System.nanoTime();
        try (Connection connection = dataSource.getConnection();
             PreparedStatement statement = connection.prepareStatement(parsedSql.jdbcSql())) {
            bind(statement, parsedSql.parameterNames(), parameters);
            try (ResultSet resultSet = statement.executeQuery()) {
                List<T> rows = RowMappers.mapAll(resultSet, type);
                log(context, elapsedMilliseconds(start), true, null, rows.size(), null);
                return rows;
            }
        } catch (SQLException ex) {
            log(context, elapsedMilliseconds(start), false, null, null, ex);
            throw new TyouquDatabaseException("Database query failed.", ex);
        }
    }

    public <T> T querySingleOrDefault(String sql, Map<String, ?> parameters, Class<T> type) {
        List<T> rows = query(sql, parameters, type);
        if (rows.isEmpty()) {
            return null;
        }
        if (rows.size() > 1) {
            throw new TyouquDatabaseException("Expected zero or one row, but query returned " + rows.size() + " rows.");
        }
        return rows.get(0);
    }

    private void bind(PreparedStatement statement, List<String> names, Map<String, ?> parameters) throws SQLException {
        statement.setQueryTimeout(options.commandTimeoutSeconds());
        Map<String, ?> safeParameters = parameters == null ? Map.of() : parameters;
        for (int i = 0; i < names.size(); i++) {
            String name = names.get(i);
            if (!safeParameters.containsKey(name)) {
                throw new TyouquDatabaseException("SQL parameter was not provided. Parameter=" + name);
            }
            statement.setObject(i + 1, safeParameters.get(name));
        }
    }

    private SqlExecutionContext createContext(String sql, Map<String, ?> parameters, SqlExecutionKind kind) {
        return new SqlExecutionContext(sql, parameters, options.provider(), kind, null);
    }

    private void beforeExecute(SqlExecutionContext context) {
        for (SqlInterceptor interceptor : interceptors) {
            interceptor.beforeExecute(context);
        }
    }

    private void log(
        SqlExecutionContext context,
        long elapsedMilliseconds,
        boolean succeeded,
        Integer affectedRows,
        Integer returnedRows,
        Exception exception
    ) {
        if (!options.sqlLogging().enabled() || executionLoggers.isEmpty()) {
            return;
        }

        boolean slowSql = elapsedMilliseconds >= options.sqlLogging().slowSqlThresholdMs();
        if (options.sqlLogging().logOnlySlowSql() && !slowSql) {
            return;
        }

        SqlExecutionLog log = new SqlExecutionLog(
            context.sqlId(),
            options.sqlLogging().logSql() ? context.sql() : null,
            options.sqlLogging().logParameters() && options.enableSensitiveLogging()
                ? context.parameters()
                : null,
            options.provider(),
            context.kind(),
            elapsedMilliseconds,
            affectedRows,
            returnedRows,
            slowSql,
            succeeded,
            exception == null ? null : exception.getMessage(),
            OffsetDateTime.now()
        );

        for (SqlExecutionLogger executionLogger : executionLoggers) {
            executionLogger.log(log);
        }
    }

    private static long elapsedMilliseconds(long startNanos) {
        return (System.nanoTime() - startNanos) / 1_000_000;
    }

    private static List<SqlInterceptor> defaultInterceptors(DatabaseOptions options) {
        DatabaseOptions safeOptions = options == null ? DatabaseOptions.builder().build() : options;
        return List.of(new FullTableOperationInterceptor(safeOptions.safety()));
    }
}
