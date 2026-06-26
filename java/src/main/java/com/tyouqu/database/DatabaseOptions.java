package com.tyouqu.database;

public record DatabaseOptions(
    DatabaseProvider provider,
    int commandTimeoutSeconds,
    boolean enableSensitiveLogging,
    SqlTemplateOptions sqlTemplates
) {
    public DatabaseOptions {
        provider = provider == null ? DatabaseProvider.SQL_SERVER : provider;
        commandTimeoutSeconds = commandTimeoutSeconds <= 0 ? 30 : commandTimeoutSeconds;
        sqlTemplates = sqlTemplates == null ? SqlTemplateOptions.builder().build() : sqlTemplates;
    }

    public static Builder builder() {
        return new Builder();
    }

    public static final class Builder {
        private DatabaseProvider provider = DatabaseProvider.SQL_SERVER;
        private int commandTimeoutSeconds = 30;
        private boolean enableSensitiveLogging;
        private SqlTemplateOptions sqlTemplates = SqlTemplateOptions.builder().build();

        public Builder provider(DatabaseProvider provider) {
            this.provider = provider;
            return this;
        }

        public Builder commandTimeoutSeconds(int commandTimeoutSeconds) {
            this.commandTimeoutSeconds = commandTimeoutSeconds;
            return this;
        }

        public Builder enableSensitiveLogging(boolean enableSensitiveLogging) {
            this.enableSensitiveLogging = enableSensitiveLogging;
            return this;
        }

        public Builder sqlTemplates(SqlTemplateOptions sqlTemplates) {
            this.sqlTemplates = sqlTemplates;
            return this;
        }

        public DatabaseOptions build() {
            return new DatabaseOptions(provider, commandTimeoutSeconds, enableSensitiveLogging, sqlTemplates);
        }
    }
}
