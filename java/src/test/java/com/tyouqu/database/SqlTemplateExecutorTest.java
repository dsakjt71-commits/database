package com.tyouqu.database;

import org.h2.jdbcx.JdbcDataSource;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import javax.sql.DataSource;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;

class SqlTemplateExecutorTest {
    @TempDir
    Path tempDir;

    @Test
    void executesNamedParametersAndMapsRecords() throws IOException {
        SqlTemplateExecutor executor = createExecutor();
        executor.executeById("demo.user.create", Map.of("id", 1, "name", "Alice"));

        UserRow user = executor.querySingleOrDefaultById("demo.user.getById", Map.of("id", 1), UserRow.class);

        assertEquals(new UserRow(1, "Alice"), user);
    }

    @Test
    void queriesPagedResult() throws IOException {
        SqlTemplateExecutor executor = createExecutor();
        executor.executeById("demo.user.create", Map.of("id", 1, "name", "Alice"));
        executor.executeById("demo.user.create", Map.of("id", 2, "name", "Bob"));
        executor.executeById("demo.user.create", Map.of("id", 3, "name", "Cindy"));

        PagedResult<UserRow> result = executor.queryPagedById("demo.user.list", Map.of(), new PageRequest(2, 1), UserRow.class);

        assertEquals(3, result.totalCount());
        assertEquals(List.of(new UserRow(2, "Bob")), result.items());
    }

    private SqlTemplateExecutor createExecutor() throws IOException {
        Files.createDirectories(tempDir.resolve("sqlite/demo"));
        Files.writeString(tempDir.resolve("sqlite/demo/demo.user.create.sql"), "insert into users(id, name) values(:id, :name)");
        Files.writeString(tempDir.resolve("sqlite/demo/demo.user.getById.sql"), "select id, name from users where id = :id");
        Files.writeString(tempDir.resolve("sqlite/demo/demo.user.list.sql"), "select id, name from users order by id");

        DataSource dataSource = createDataSource();
        JdbcDbExecutor db = new JdbcDbExecutor(dataSource);
        db.execute("create table users(id int primary key, name varchar(64))", Map.of());

        DatabaseOptions options = DatabaseOptions.builder()
            .provider(DatabaseProvider.SQLITE)
            .sqlTemplates(SqlTemplateOptions.builder().rootPath(tempDir.toString()).build())
            .build();
        return new SqlTemplateExecutor(db, new FileSqlTemplateStore(options), Dialects.forProvider(options.provider()));
    }

    private static DataSource createDataSource() {
        JdbcDataSource dataSource = new JdbcDataSource();
        dataSource.setURL("jdbc:h2:mem:" + System.nanoTime() + ";MODE=MySQL;DB_CLOSE_DELAY=-1");
        return dataSource;
    }

    record UserRow(int id, String name) {
    }
}
