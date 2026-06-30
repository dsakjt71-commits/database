from __future__ import annotations

import logging
import re
from dataclasses import dataclass, field
from datetime import datetime, timezone
from enum import Enum
from typing import Mapping, Protocol

from .exceptions import TyouquDatabaseException
from .models import DatabaseProvider, SqlSafetyOptions


class SqlExecutionKind(Enum):
    EXECUTE = "execute"
    QUERY = "query"
    QUERY_SINGLE = "query_single"


@dataclass(frozen=True)
class SqlExecutionContext:
    sql: str
    parameters: Mapping | None
    provider: DatabaseProvider
    kind: SqlExecutionKind
    sql_id: str | None = None


@dataclass(frozen=True)
class SqlExecutionLog:
    provider: DatabaseProvider
    kind: SqlExecutionKind
    elapsed_milliseconds: int
    slow_sql: bool
    succeeded: bool
    sql_id: str | None = None
    sql: str | None = None
    parameters: Mapping | None = None
    affected_rows: int | None = None
    returned_rows: int | None = None
    error_message: str | None = None
    executed_at: datetime = field(default_factory=lambda: datetime.now(timezone.utc))


class SqlExecutionLogger(Protocol):
    def log(self, entry: SqlExecutionLog) -> None:
        ...


class SqlInterceptor(Protocol):
    def before_execute(self, context: SqlExecutionContext) -> None:
        ...


class LoggingSqlExecutionLogger:
    def __init__(self, logger: logging.Logger | None = None):
        self._logger = logger or logging.getLogger("tyouqu_database.sql")

    def log(self, entry: SqlExecutionLog) -> None:
        level = logging.INFO if entry.succeeded else logging.WARNING
        self._logger.log(
            level,
            "SQL %s. provider=%s kind=%s elapsed_ms=%s slow_sql=%s affected_rows=%s returned_rows=%s error=%s sql=%s parameters=%s",
            "executed" if entry.succeeded else "failed",
            entry.provider.value,
            entry.kind.value,
            entry.elapsed_milliseconds,
            entry.slow_sql,
            entry.affected_rows,
            entry.returned_rows,
            entry.error_message,
            entry.sql,
            entry.parameters,
        )


class FullTableOperationInterceptor:
    def __init__(self, options: SqlSafetyOptions | None = None):
        self._options = options or SqlSafetyOptions()

    def before_execute(self, context: SqlExecutionContext) -> None:
        normalized = normalize_sql(context.sql)
        if self._options.block_full_table_update and is_full_table_update(normalized):
            raise TyouquDatabaseException("Unsafe SQL was blocked: UPDATE without WHERE is not allowed.", sql_id=context.sql_id)
        if self._options.block_full_table_delete and is_full_table_delete(normalized):
            raise TyouquDatabaseException("Unsafe SQL was blocked: DELETE without WHERE is not allowed.", sql_id=context.sql_id)


def normalize_sql(sql: str) -> str:
    result: list[str] = []
    in_single_quote = False
    in_double_quote = False
    in_line_comment = False
    in_block_comment = False
    index = 0

    while index < len(sql):
        current = sql[index]
        next_char = sql[index + 1] if index + 1 < len(sql) else ""

        if in_line_comment:
            if current in "\r\n":
                in_line_comment = False
                result.append(" ")
            index += 1
            continue

        if in_block_comment:
            if current == "*" and next_char == "/":
                in_block_comment = False
                result.append(" ")
                index += 2
                continue
            index += 1
            continue

        if not in_single_quote and not in_double_quote and current == "-" and next_char == "-":
            in_line_comment = True
            index += 2
            continue

        if not in_single_quote and not in_double_quote and current == "/" and next_char == "*":
            in_block_comment = True
            index += 2
            continue

        if not in_double_quote and current == "'":
            in_single_quote = not in_single_quote
            result.append(" ")
            index += 1
            continue

        if not in_single_quote and current == '"':
            in_double_quote = not in_double_quote
            result.append(" ")
            index += 1
            continue

        result.append(" " if in_single_quote or in_double_quote else current.lower())
        index += 1

    return re.sub(r"\s+", " ", "".join(result)).strip()


def is_full_table_update(normalized_sql: str) -> bool:
    return normalized_sql.startswith("update ") and not _contains_where(normalized_sql)


def is_full_table_delete(normalized_sql: str) -> bool:
    return (
        normalized_sql.startswith("delete from ")
        or normalized_sql == "delete"
        or normalized_sql.startswith("delete ")
    ) and not _contains_where(normalized_sql)


def _contains_where(normalized_sql: str) -> bool:
    return "where" in normalized_sql.split()
