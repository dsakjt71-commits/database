package com.tyouqu.database;

public record SqlTemplateOptions(
    String rootPath,
    boolean failOnDuplicateSqlId,
    boolean failOnMissingSqlId
) {
    public SqlTemplateOptions {
        rootPath = rootPath == null || rootPath.isBlank() ? "sql" : rootPath;
    }

    public static Builder builder() {
        return new Builder();
    }

    public static final class Builder {
        private String rootPath = "sql";
        private boolean failOnDuplicateSqlId = true;
        private boolean failOnMissingSqlId = true;

        public Builder rootPath(String rootPath) {
            this.rootPath = rootPath;
            return this;
        }

        public Builder failOnDuplicateSqlId(boolean failOnDuplicateSqlId) {
            this.failOnDuplicateSqlId = failOnDuplicateSqlId;
            return this;
        }

        public Builder failOnMissingSqlId(boolean failOnMissingSqlId) {
            this.failOnMissingSqlId = failOnMissingSqlId;
            return this;
        }

        public SqlTemplateOptions build() {
            return new SqlTemplateOptions(rootPath, failOnDuplicateSqlId, failOnMissingSqlId);
        }
    }
}
