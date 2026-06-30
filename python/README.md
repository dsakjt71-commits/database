# Tyouqu.Database for Python

Python implementation of the `database` component. It keeps the same core rules as the Java and .NET versions:

- SQL templates are stored as `.sql` files.
- SQL ID is the filename without `.sql`.
- `sql/common` is loaded first.
- `sql/{provider}` overrides `common`.
- Duplicate SQL IDs in the same scope fail by default.
- Empty SQL files and legacy `-- @id` markers fail.
- Named parameters use `:name`.

## Install for development

```powershell
cd python
python -m pip install -e .[dev]
pytest
```

## Example

```python
import sqlite3
from tyouqu_database import (
    DatabaseOptions,
    DatabaseProvider,
    FileSqlTemplateStore,
    PageRequest,
    SqlExecutionLogOptions,
    SqlTemplateExecutor,
    SqliteDbExecutor,
    dialect_for_provider,
)

logs = []
options = DatabaseOptions(
    provider=DatabaseProvider.SQLITE,
    connection_factory=lambda: sqlite3.connect("app.db"),
    sql_logging=SqlExecutionLogOptions(
        enabled=True,
        log_sql=True,
        log_parameters=False,
        slow_sql_threshold_ms=500,
    ),
)

store = FileSqlTemplateStore(options)
db = SqliteDbExecutor(
    options.connection_factory,
    options,
    execution_loggers=[logs.append],
)
sql = SqlTemplateExecutor(db, store, dialect_for_provider(options.provider))

user = sql.query_single_or_default_by_id("auth.user.getById", {"userId": 1})
page = sql.query_paged_by_id("auth.user.list", {}, PageRequest(1, 20))
```

## SQL execution logging and safety

`SqliteDbExecutor` supports execution logs and slow SQL detection. You can pass custom loggers through `execution_loggers`; each logger can be an object with a `log(entry)` method or a callable such as `logs.append`.

The default safety interceptor blocks full-table `delete` and `update` statements without a `where` clause:

```sql
delete from users
```

```sql
update users set status = 0
```

Use explicit conditions for destructive statements:

```sql
delete from users where id = :id
```
