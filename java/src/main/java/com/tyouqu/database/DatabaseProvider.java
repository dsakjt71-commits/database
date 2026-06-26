package com.tyouqu.database;

public enum DatabaseProvider {
    SQL_SERVER("sqlserver"),
    MYSQL("mysql"),
    POSTGRESQL("postgresql"),
    SQLITE("sqlite");

    private final String directoryName;

    DatabaseProvider(String directoryName) {
        this.directoryName = directoryName;
    }

    public String directoryName() {
        return directoryName;
    }
}
