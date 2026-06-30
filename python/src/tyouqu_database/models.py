from dataclasses import dataclass, field
from enum import Enum
from typing import Callable, Optional


class DatabaseProvider(Enum):
    SQL_SERVER = "sqlserver"
    MYSQL = "mysql"
    POSTGRESQL = "postgresql"
    SQLITE = "sqlite"

    @property
    def directory_name(self) -> str:
        return self.value


@dataclass(frozen=True)
class SqlTemplateOptions:
    root_path: str = "sql"
    fail_on_duplicate_sql_id: bool = True
    fail_on_missing_sql_id: bool = True

    def __post_init__(self):
        if self.root_path is None or str(self.root_path).strip() == "":
            object.__setattr__(self, "root_path", "sql")


@dataclass(frozen=True)
class SqlExecutionLogOptions:
    enabled: bool = False
    log_sql: bool = True
    log_parameters: bool = False
    log_only_slow_sql: bool = False
    slow_sql_threshold_ms: int = 500

    def __post_init__(self):
        if self.slow_sql_threshold_ms <= 0:
            object.__setattr__(self, "slow_sql_threshold_ms", 500)


@dataclass(frozen=True)
class SqlSafetyOptions:
    block_full_table_update: bool = True
    block_full_table_delete: bool = True


@dataclass(frozen=True)
class DatabaseOptions:
    provider: DatabaseProvider = DatabaseProvider.SQL_SERVER
    connection_factory: Optional[Callable[[], object]] = None
    command_timeout_seconds: int = 30
    enable_sensitive_logging: bool = False
    sql_templates: SqlTemplateOptions = field(default_factory=SqlTemplateOptions)
    sql_logging: SqlExecutionLogOptions = field(default_factory=SqlExecutionLogOptions)
    safety: SqlSafetyOptions = field(default_factory=SqlSafetyOptions)

    def __post_init__(self):
        if self.provider is None:
            object.__setattr__(self, "provider", DatabaseProvider.SQL_SERVER)
        if isinstance(self.provider, str):
            object.__setattr__(self, "provider", DatabaseProvider(self.provider.lower()))
        if self.command_timeout_seconds <= 0:
            object.__setattr__(self, "command_timeout_seconds", 30)
        if self.sql_templates is None:
            object.__setattr__(self, "sql_templates", SqlTemplateOptions())
        if self.sql_logging is None:
            object.__setattr__(self, "sql_logging", SqlExecutionLogOptions())
        if self.safety is None:
            object.__setattr__(self, "safety", SqlSafetyOptions())


@dataclass(frozen=True)
class PageRequest:
    page_index: int
    page_size: int

    @property
    def offset(self) -> int:
        return max(self.page_index - 1, 0) * self.page_size


@dataclass(frozen=True)
class PagedResult:
    items: list
    total_count: int
    page_index: int
    page_size: int
