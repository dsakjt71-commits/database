package com.tyouqu.database;

public record SqlSafetyOptions(
    boolean blockFullTableUpdate,
    boolean blockFullTableDelete
) {
    public static SqlSafetyOptions defaults() {
        return new SqlSafetyOptions(true, true);
    }
}
