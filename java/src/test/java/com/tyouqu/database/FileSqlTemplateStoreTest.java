package com.tyouqu.database;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;

class FileSqlTemplateStoreTest {
    @TempDir
    Path tempDir;

    @Test
    void providerTemplateOverridesCommonTemplate() throws IOException {
        Files.createDirectories(tempDir.resolve("common/auth"));
        Files.createDirectories(tempDir.resolve("mysql/auth"));
        Files.writeString(tempDir.resolve("common/auth/auth.app.getById.sql"), "select 'common'");
        Files.writeString(tempDir.resolve("mysql/auth/auth.app.getById.sql"), "select 'mysql'");

        DatabaseOptions options = DatabaseOptions.builder()
            .provider(DatabaseProvider.MYSQL)
            .sqlTemplates(SqlTemplateOptions.builder().rootPath(tempDir.toString()).build())
            .build();

        FileSqlTemplateStore store = new FileSqlTemplateStore(options);

        assertEquals("select 'mysql'", store.getRequiredSql("auth.app.getById"));
    }
}
