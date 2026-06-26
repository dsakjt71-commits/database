package com.tyouqu.database;

public class TyouquDatabaseException extends RuntimeException {
    private String sqlId;
    private String provider;

    public TyouquDatabaseException(String message) {
        super(message);
    }

    public TyouquDatabaseException(String message, Throwable cause) {
        super(message, cause);
    }

    public String sqlId() {
        return sqlId;
    }

    public TyouquDatabaseException sqlId(String sqlId) {
        this.sqlId = sqlId;
        return this;
    }

    public String provider() {
        return provider;
    }

    public TyouquDatabaseException provider(String provider) {
        this.provider = provider;
        return this;
    }
}
