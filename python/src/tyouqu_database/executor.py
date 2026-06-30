from abc import ABC, abstractmethod
from collections.abc import Callable, Mapping
from contextlib import closing
from dataclasses import is_dataclass
from time import perf_counter

from .dialects import SqlDialect
from .exceptions import TyouquDatabaseException
from .models import DatabaseOptions, DatabaseProvider, PageRequest, PagedResult
from .named_parameter_sql import parse_named_parameter_sql
from .pipeline import (
    FullTableOperationInterceptor,
    LoggingSqlExecutionLogger,
    SqlExecutionContext,
    SqlExecutionKind,
    SqlExecutionLog,
    SqlExecutionLogger,
    SqlInterceptor,
)
from .store import SqlTemplateStore


class DbExecutor(ABC):
    @abstractmethod
    def execute(self, sql: str, parameters: Mapping | None = None) -> int:
        raise NotImplementedError

    @abstractmethod
    def query(self, sql: str, parameters: Mapping | None = None, row_mapper: Callable | type | None = None) -> list:
        raise NotImplementedError

    def query_single_or_default(self, sql: str, parameters: Mapping | None = None, row_mapper: Callable | type | None = None):
        rows = self.query(sql, parameters, row_mapper)
        if not rows:
            return None
        if len(rows) > 1:
            raise TyouquDatabaseException(f"Expected zero or one row, but query returned {len(rows)} rows.")
        return rows[0]


class SqliteDbExecutor(DbExecutor):
    def __init__(
        self,
        connection_factory: Callable[[], object],
        options: DatabaseOptions | None = None,
        interceptors: list[SqlInterceptor] | None = None,
        execution_loggers: list[SqlExecutionLogger] | None = None,
    ):
        self._connection_factory = connection_factory
        self._options = options or DatabaseOptions(provider=DatabaseProvider.SQLITE, connection_factory=connection_factory)
        self._interceptors = (
            [FullTableOperationInterceptor(self._options.safety)] if interceptors is None else list(interceptors)
        )
        self._execution_loggers = (
            [LoggingSqlExecutionLogger()] if execution_loggers is None else list(execution_loggers)
        )

    def execute(self, sql: str, parameters: Mapping | None = None) -> int:
        context = self._create_context(sql, parameters, SqlExecutionKind.EXECUTE)
        self._before_execute(context)
        parsed_sql, values = _prepare(sql, parameters)
        started_at = perf_counter()
        try:
            with closing(self._connection_factory()) as connection:
                with connection:
                    with closing(connection.execute(parsed_sql, values)) as cursor:
                        affected_rows = cursor.rowcount
                        self._log(context, _elapsed_ms(started_at), True, affected_rows, None, None)
                        return affected_rows
        except Exception as ex:
            self._log(context, _elapsed_ms(started_at), False, None, None, ex)
            raise TyouquDatabaseException("Database execute failed.", ex) from ex

    def query(self, sql: str, parameters: Mapping | None = None, row_mapper: Callable | type | None = None) -> list:
        context = self._create_context(sql, parameters, SqlExecutionKind.QUERY)
        self._before_execute(context)
        parsed_sql, values = _prepare(sql, parameters)
        started_at = perf_counter()
        try:
            with closing(self._connection_factory()) as connection:
                with closing(connection.execute(parsed_sql, values)) as cursor:
                    columns = [description[0] for description in cursor.description or []]
                    rows = cursor.fetchall()
                    mapped_rows = [_map_row(columns, row, row_mapper) for row in rows]
                    self._log(context, _elapsed_ms(started_at), True, None, len(mapped_rows), None)
                    return mapped_rows
        except Exception as ex:
            self._log(context, _elapsed_ms(started_at), False, None, None, ex)
            raise TyouquDatabaseException("Database query failed.", ex) from ex

    def _create_context(self, sql: str, parameters: Mapping | None, kind: SqlExecutionKind) -> SqlExecutionContext:
        return SqlExecutionContext(sql, parameters, self._options.provider, kind)

    def _before_execute(self, context: SqlExecutionContext) -> None:
        for interceptor in self._interceptors:
            interceptor.before_execute(context)

    def _log(
        self,
        context: SqlExecutionContext,
        elapsed_milliseconds: int,
        succeeded: bool,
        affected_rows: int | None,
        returned_rows: int | None,
        exception: Exception | None,
    ) -> None:
        if not self._options.sql_logging.enabled or not self._execution_loggers:
            return

        slow_sql = elapsed_milliseconds >= self._options.sql_logging.slow_sql_threshold_ms
        if self._options.sql_logging.log_only_slow_sql and not slow_sql:
            return

        entry = SqlExecutionLog(
            sql_id=context.sql_id,
            sql=context.sql if self._options.sql_logging.log_sql else None,
            parameters=(
                context.parameters
                if self._options.sql_logging.log_parameters and self._options.enable_sensitive_logging
                else None
            ),
            provider=self._options.provider,
            kind=context.kind,
            elapsed_milliseconds=elapsed_milliseconds,
            affected_rows=affected_rows,
            returned_rows=returned_rows,
            slow_sql=slow_sql,
            succeeded=succeeded,
            error_message=None if exception is None else str(exception),
        )
        for execution_logger in self._execution_loggers:
            if callable(execution_logger):
                execution_logger(entry)
            else:
                execution_logger.log(entry)


class SqlTemplateExecutor:
    def __init__(self, db: DbExecutor, sql_store: SqlTemplateStore, dialect: SqlDialect):
        self._db = db
        self._sql_store = sql_store
        self._dialect = dialect

    def execute_by_id(self, sql_id: str, parameters: Mapping | None = None) -> int:
        try:
            return self._db.execute(self._sql_store.get_required_sql(sql_id), parameters)
        except TyouquDatabaseException as ex:
            if ex.sql_id is None:
                ex.sql_id = sql_id
            raise

    def query_by_id(self, sql_id: str, parameters: Mapping | None = None, row_mapper: Callable | type | None = None) -> list:
        try:
            return self._db.query(self._sql_store.get_required_sql(sql_id), parameters, row_mapper)
        except TyouquDatabaseException as ex:
            if ex.sql_id is None:
                ex.sql_id = sql_id
            raise

    def query_single_or_default_by_id(
        self, sql_id: str, parameters: Mapping | None = None, row_mapper: Callable | type | None = None
    ):
        try:
            return self._db.query_single_or_default(self._sql_store.get_required_sql(sql_id), parameters, row_mapper)
        except TyouquDatabaseException as ex:
            if ex.sql_id is None:
                ex.sql_id = sql_id
            raise

    def query_paged_by_id(
        self, sql_id: str, parameters: Mapping | None, page: PageRequest, row_mapper: Callable | type | None = None
    ) -> PagedResult:
        if page.page_index < 1:
            raise ValueError("page_index must be greater than or equal to 1.")
        if page.page_size < 1:
            raise ValueError("page_size must be greater than or equal to 1.")

        sql = self._sql_store.get_required_sql(sql_id)
        count_sql = f"select count(1) from ({_remove_trailing_order_by(_trim_trailing_semicolon(sql))}) as _paged_source"
        paged_sql = self._dialect.build_paged_sql(sql, page.offset, page.page_size)
        total = self._db.query_single_or_default(count_sql, parameters)
        items = self._db.query(paged_sql, parameters, row_mapper)
        total_count = 0 if total is None else _first_value(total)
        return PagedResult(items, int(total_count), page.page_index, page.page_size)


def _prepare(sql: str, parameters: Mapping | None) -> tuple[str, list]:
    parsed = parse_named_parameter_sql(sql)
    safe_parameters = {} if parameters is None else parameters
    values = []
    for name in parsed.parameter_names:
        if name not in safe_parameters:
            raise TyouquDatabaseException(f"SQL parameter was not provided. Parameter={name}")
        values.append(safe_parameters[name])
    return parsed.sql, values


def _map_row(columns: list[str], row, row_mapper: Callable | type | None):
    values = dict(zip(columns, row))
    if row_mapper is None:
        return values
    if row_mapper in (dict, Mapping):
        return values
    if row_mapper in (tuple, list):
        return row_mapper(row)
    if row_mapper in (int, float, str, bool):
        return None if not row else row_mapper(row[0])
    if is_dataclass(row_mapper):
        return row_mapper(**values)
    return row_mapper(values)


def _trim_trailing_semicolon(sql: str) -> str:
    return sql.strip().removesuffix(";")


def _remove_trailing_order_by(sql: str) -> str:
    index = sql.lower().rfind("order by")
    return sql if index < 0 else sql[:index].rstrip()


def _first_value(row):
    if isinstance(row, Mapping):
        return next(iter(row.values()))
    if isinstance(row, (tuple, list)):
        return row[0]
    return row


def _elapsed_ms(started_at: float) -> int:
    return int((perf_counter() - started_at) * 1000)
