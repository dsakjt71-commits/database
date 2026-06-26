package com.tyouqu.database;

import javax.sql.DataSource;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;
import java.util.Map;

public final class JdbcDbExecutor implements DbExecutor {
    private final DataSource dataSource;
    private final int commandTimeoutSeconds;

    public JdbcDbExecutor(DataSource dataSource) {
        this(dataSource, 30);
    }

    public JdbcDbExecutor(DataSource dataSource, int commandTimeoutSeconds) {
        this.dataSource = dataSource;
        this.commandTimeoutSeconds = commandTimeoutSeconds <= 0 ? 30 : commandTimeoutSeconds;
    }

    public int execute(String sql, Map<String, ?> parameters) {
        NamedParameterSql.ParsedSql parsedSql = NamedParameterSql.parse(sql);
        try (Connection connection = dataSource.getConnection();
             PreparedStatement statement = connection.prepareStatement(parsedSql.jdbcSql())) {
            bind(statement, parsedSql.parameterNames(), parameters);
            return statement.executeUpdate();
        } catch (SQLException ex) {
            throw new TyouquDatabaseException("Database execute failed.", ex);
        }
    }

    public <T> List<T> query(String sql, Map<String, ?> parameters, Class<T> type) {
        NamedParameterSql.ParsedSql parsedSql = NamedParameterSql.parse(sql);
        try (Connection connection = dataSource.getConnection();
             PreparedStatement statement = connection.prepareStatement(parsedSql.jdbcSql())) {
            bind(statement, parsedSql.parameterNames(), parameters);
            try (ResultSet resultSet = statement.executeQuery()) {
                return RowMappers.mapAll(resultSet, type);
            }
        } catch (SQLException ex) {
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
        statement.setQueryTimeout(commandTimeoutSeconds);
        Map<String, ?> safeParameters = parameters == null ? Map.of() : parameters;
        for (int i = 0; i < names.size(); i++) {
            String name = names.get(i);
            if (!safeParameters.containsKey(name)) {
                throw new TyouquDatabaseException("SQL parameter was not provided. Parameter=" + name);
            }
            statement.setObject(i + 1, safeParameters.get(name));
        }
    }
}
