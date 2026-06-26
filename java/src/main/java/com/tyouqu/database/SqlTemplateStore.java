package com.tyouqu.database;

import java.util.Optional;

public interface SqlTemplateStore {
    String getRequiredSql(String sqlId);

    Optional<String> tryGetSql(String sqlId);

    void reload();
}
