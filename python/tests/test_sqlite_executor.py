import sqlite3
from contextlib import closing

from tyouqu_database import (
    DatabaseOptions,
    DatabaseProvider,
    FileSqlTemplateStore,
    PageRequest,
    SqlExecutionLogOptions,
    SqlTemplateExecutor,
    SqlTemplateOptions,
    SqliteDbExecutor,
    TyouquDatabaseException,
    dialect_for_provider,
)


def test_sql_template_executor_queries_sqlite(tmp_path):
    db_path = tmp_path / "app.db"
    with closing(sqlite3.connect(db_path)) as connection:
        with connection:
            connection.execute("create table users(id integer primary key, name text)")
            connection.executemany("insert into users(name) values(?)", [("Ada",), ("Linus",), ("Grace",)])

    sql_root = tmp_path / "sql"
    (sql_root / "sqlite").mkdir(parents=True)
    (sql_root / "sqlite" / "user.byId.sql").write_text(
        "select id, name from users where id = :id",
        encoding="utf-8",
    )
    (sql_root / "sqlite" / "user.list.sql").write_text(
        "select id, name from users order by id",
        encoding="utf-8",
    )

    options = DatabaseOptions(
        provider=DatabaseProvider.SQLITE,
        connection_factory=lambda: sqlite3.connect(db_path),
        sql_templates=SqlTemplateOptions(root_path=str(sql_root)),
    )
    executor = SqlTemplateExecutor(
        SqliteDbExecutor(options.connection_factory),
        FileSqlTemplateStore(options),
        dialect_for_provider(options.provider),
    )

    row = executor.query_single_or_default_by_id("user.byId", {"id": 1})
    page = executor.query_paged_by_id("user.list", {}, PageRequest(2, 1))

    assert row == {"id": 1, "name": "Ada"}
    assert page.total_count == 3
    assert page.items == [{"id": 2, "name": "Linus"}]


def test_sqlite_executor_blocks_full_table_delete(tmp_path):
    db_path = tmp_path / "app.db"
    with closing(sqlite3.connect(db_path)) as connection:
        with connection:
            connection.execute("create table users(id integer primary key, name text)")
            connection.execute("insert into users(name) values(?)", ("Ada",))

    options = DatabaseOptions(
        provider=DatabaseProvider.SQLITE,
        connection_factory=lambda: sqlite3.connect(db_path),
    )
    db = SqliteDbExecutor(options.connection_factory, options)

    try:
        db.execute("delete from users")
    except TyouquDatabaseException as ex:
        assert "DELETE without WHERE" in str(ex)
    else:
        raise AssertionError("Expected full table delete to be blocked.")


def test_sqlite_executor_writes_execution_log_when_enabled(tmp_path):
    logs = []
    db_path = tmp_path / "app.db"
    options = DatabaseOptions(
        provider=DatabaseProvider.SQLITE,
        connection_factory=lambda: sqlite3.connect(db_path),
        enable_sensitive_logging=True,
        sql_logging=SqlExecutionLogOptions(enabled=True, log_sql=True, log_parameters=True),
    )
    db = SqliteDbExecutor(options.connection_factory, options, execution_loggers=[logs.append])

    db.execute("create table users(id integer primary key, name text)")

    assert len(logs) == 1
    assert logs[0].succeeded is True
    assert logs[0].sql == "create table users(id integer primary key, name text)"
    assert logs[0].elapsed_milliseconds >= 0
