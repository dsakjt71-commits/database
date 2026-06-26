package com.tyouqu.database;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Optional;
import java.util.Set;
import java.util.stream.Stream;

public final class FileSqlTemplateStore implements SqlTemplateStore {
    private final DatabaseOptions options;
    private volatile Map<String, String> templates = Map.of();

    public FileSqlTemplateStore(DatabaseOptions options) {
        this.options = options;
        reload();
    }

    public String getRequiredSql(String sqlId) {
        return tryGetSql(sqlId).orElseThrow(() -> new TyouquDatabaseException("SQL template was not found. SqlId=" + sqlId)
            .sqlId(sqlId)
            .provider(options.provider().name()));
    }

    public Optional<String> tryGetSql(String sqlId) {
        if (sqlId == null) {
            return Optional.empty();
        }
        return Optional.ofNullable(templates.get(sqlId.toLowerCase()));
    }

    public void reload() {
        Path rootPath = resolveRootPath(options.sqlTemplates().rootPath());
        if (!Files.isDirectory(rootPath)) {
            throw new TyouquDatabaseException("SQL template root path does not exist. Path=" + rootPath);
        }

        Map<String, String> loaded = new HashMap<>();
        loadDirectory(rootPath.resolve("common"), loaded, false);
        loadDirectory(rootPath.resolve(options.provider().directoryName()), loaded, true);
        templates = Map.copyOf(loaded);
    }

    private void loadDirectory(Path directory, Map<String, String> loaded, boolean allowOverride) {
        if (!Files.isDirectory(directory)) {
            return;
        }

        Set<String> currentScopeIds = new HashSet<>();
        try (Stream<Path> files = Files.walk(directory)) {
            files.filter(path -> Files.isRegularFile(path) && path.getFileName().toString().endsWith(".sql"))
                .forEach(file -> {
                    SqlTemplate template = parseFile(file);
                    String key = template.sqlId().toLowerCase();
                    if (!currentScopeIds.add(key) && options.sqlTemplates().failOnDuplicateSqlId()) {
                        throw new TyouquDatabaseException("Duplicate SQL template id was found in the same scope. SqlId=" + template.sqlId() + ", File=" + file);
                    }
                    if (!allowOverride && loaded.containsKey(key) && options.sqlTemplates().failOnDuplicateSqlId()) {
                        throw new TyouquDatabaseException("Duplicate SQL template id was found. SqlId=" + template.sqlId() + ", File=" + file);
                    }
                    loaded.put(key, template.sql());
                });
        } catch (IOException ex) {
            throw new TyouquDatabaseException("Failed to load SQL templates from " + directory, ex);
        }
    }

    private static SqlTemplate parseFile(Path file) {
        try {
            String sql = Files.readString(file, StandardCharsets.UTF_8).trim();
            if (sql.isBlank()) {
                throw new TyouquDatabaseException("SQL template file is empty. File=" + file);
            }
            if (sql.toLowerCase().contains("-- @id ")) {
                throw new TyouquDatabaseException("SQL template file must use filename as sql id. Remove -- @id marker. File=" + file);
            }
            String fileName = file.getFileName().toString();
            String sqlId = fileName.substring(0, fileName.length() - ".sql".length());
            return new SqlTemplate(sqlId, sql);
        } catch (IOException ex) {
            throw new TyouquDatabaseException("Failed to read SQL template file. File=" + file, ex);
        }
    }

    private static Path resolveRootPath(String rootPath) {
        Path path = Path.of(rootPath);
        return path.isAbsolute() ? path : Path.of("").toAbsolutePath().resolve(path).normalize();
    }

    private record SqlTemplate(String sqlId, String sql) {
    }
}
