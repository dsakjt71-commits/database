# SQL 模板规范

## 一个文件一个 SQL ID

SQL ID 等于文件名去掉 `.sql`。

正确：

```text
auth.app.getById.sql
```

错误：

```sql
-- @id auth.app.getById
select ...
```

文件中如果包含 `-- @id `，组件会直接报错。

这条规则对 SQL Server、MySQL、PostgreSQL、SQLite 都一样。组件不会从 SQL 文件内容中读取 SQL ID，只使用文件名作为 SQL ID。

## Provider 目录

```text
sql/common
sql/sqlserver
sql/mysql
sql/postgresql
sql/sqlite
```

示例：

```text
sql/sqlserver/auth/auth.app.getById.sql
sql/mysql/auth/auth.app.getById.sql
sql/postgresql/auth/auth.app.getById.sql
sql/sqlite/auth/auth.app.getById.sql
```

如果 SQL 完全通用，可以放到：

```text
sql/common/auth/auth.app.getById.sql
```

Provider 专用 SQL 会覆盖 `common` 中同名 SQL ID。

调用方不需要在 SQL ID 中携带数据库类型：

```csharp
await db.QuerySingleOrDefaultByIdAsync<App>(
    "auth.app.getById",
    new { AppId = 1 });
```

组件会根据当前 `DatabaseOptions.Provider` 到对应 Provider 目录查找 SQL。

## 注释

普通注释允许使用：

```sql
-- 查询应用详情
select *
from Apps
where app_id = @AppId;
```

块注释也允许：

```sql
/*
  查询应用详情
*/
select *
from Apps
where app_id = @AppId;
```

## 常用 SQL 示例

### 查询单条

文件：

```text
sql/common/auth/auth.app.getById.sql
```

内容：

```sql
select
    app_id as AppId,
    app_name as AppName,
    app_key as AppKey,
    status as Status,
    created_at as CreatedAt
from auth_apps
where app_id = @AppId;
```

### 条件列表

文件：

```text
sql/common/auth/auth.app.list.sql
```

内容：

```sql
select
    app_id as AppId,
    app_name as AppName,
    app_key as AppKey,
    status as Status,
    created_at as CreatedAt
from auth_apps
where (@Keyword is null or app_name like @Keyword or app_key like @Keyword)
  and (@Status is null or status = @Status)
order by created_at desc;
```

调用分页查询时，组件会基于这条 SQL 自动生成 count SQL 和分页 SQL。建议列表 SQL 明确写 `order by`，保证分页结果稳定。

### 判断是否存在

文件：

```text
sql/common/auth/auth.app.existsByKey.sql
```

内容：

```sql
select count(1)
from auth_apps
where app_key = @AppKey;
```

### 插入

文件：

```text
sql/common/auth/auth.app.create.sql
```

内容：

```sql
insert into auth_apps (
    app_name,
    app_key,
    status,
    created_at
) values (
    @AppName,
    @AppKey,
    @Status,
    current_timestamp
);
```

### 更新

文件：

```text
sql/common/auth/auth.app.update.sql
```

内容：

```sql
update auth_apps
set app_name = @AppName,
    status = @Status,
    updated_at = current_timestamp
where app_id = @AppId;
```

### 删除

文件：

```text
sql/common/auth/auth.app.delete.sql
```

内容：

```sql
delete from auth_apps
where app_id = @AppId;
```

### 软删除

文件：

```text
sql/common/auth/auth.app.disable.sql
```

内容：

```sql
update auth_apps
set status = 0,
    updated_at = current_timestamp
where app_id = @AppId;
```

## Provider 专用 SQL 示例

当不同数据库的语法不一致时，保留相同 SQL ID，把 SQL 放到不同 Provider 目录。

SQL Server：

```text
sql/sqlserver/auth/auth.app.lastCreated.sql
```

```sql
select top 1
    app_id as AppId,
    app_name as AppName,
    created_at as CreatedAt
from auth_apps
order by created_at desc;
```

MySQL / PostgreSQL / SQLite：

```text
sql/mysql/auth/auth.app.lastCreated.sql
sql/postgresql/auth/auth.app.lastCreated.sql
sql/sqlite/auth/auth.app.lastCreated.sql
```

```sql
select
    app_id as AppId,
    app_name as AppName,
    created_at as CreatedAt
from auth_apps
order by created_at desc
limit 1;
```

调用方仍然只使用同一个 SQL ID：

```csharp
await db.QuerySingleOrDefaultByIdAsync<App>(
    "auth.app.lastCreated");
```

## 多语句

一个文件可以写多条 SQL：

```sql
update Users
set last_login_at = current_timestamp
where user_id = @UserId;

insert into LoginLogs(user_id, created_at)
values(@UserId, current_timestamp);

select 1 as Success;
```

## 多结果集

一个 SQL 文件可以返回多个结果集：

```sql
select *
from Users
where user_id = @UserId;

select *
from UserApps
where user_id = @UserId;
```

调用：

```csharp
var result = await db.QueryMultipleByIdAsync<User, UserApp>(
    "auth.user.loginContext",
    new { UserId = userId });
```

当前封装支持 2 个和 3 个结果集。

## 执行日志和慢 SQL

组件可以在执行 SQL 时记录执行日志，用于后续排查慢 SQL 和失败 SQL。日志内容包括：

- Provider。
- 执行类型，例如执行、查询、单条查询、多结果集查询。
- SQL 文本。
- 参数。
- 执行耗时。
- 影响行数或返回行数。
- 是否超过慢 SQL 阈值。
- 是否执行成功。
- 异常信息。

是否输出 SQL 文本、是否输出参数、是否只记录慢 SQL 都由用户配置决定。参数日志默认关闭，只有显式开启参数日志并允许敏感日志时才会输出参数。

.NET 示例：

```csharp
builder.Services.AddTyouquDatabase(options =>
{
    options.SqlLogging.Enabled = true;
    options.SqlLogging.LogSql = true;
    options.SqlLogging.LogParameters = false;
    options.SqlLogging.LogOnlySlowSql = false;
    options.SqlLogging.SlowSqlThresholdMs = 500;
});
```

Java 示例：

```java
DatabaseOptions options = DatabaseOptions.builder()
    .provider(DatabaseProvider.MYSQL)
    .enableSensitiveLogging(false)
    .sqlLogging(new SqlExecutionLogOptions(true, true, false, false, 500))
    .build();

JdbcDbExecutor db = new JdbcDbExecutor(
    dataSource,
    options,
    List.of(new FullTableOperationInterceptor(options.safety())),
    List.of(new ConsoleSqlExecutionLogger())
);
```

Python 示例：

```python
logs = []
options = DatabaseOptions(
    provider=DatabaseProvider.SQLITE,
    connection_factory=lambda: sqlite3.connect("app.db"),
    enable_sensitive_logging=False,
    sql_logging=SqlExecutionLogOptions(
        enabled=True,
        log_sql=True,
        log_parameters=False,
        slow_sql_threshold_ms=500,
    ),
)

db = SqliteDbExecutor(
    options.connection_factory,
    options,
    execution_loggers=[logs.append],
)
```

用户可以实现自己的日志记录器，把日志写入本地文件、日志框架或数据库日志表。

## 安全拦截

组件内置全表 `update` 和 `delete` 操作阻断能力。默认会阻止没有 `where` 条件的 SQL：

```sql
delete from auth_apps
```

```sql
update auth_apps
set status = 0
```

允许带有明确条件的 SQL：

```sql
delete from auth_apps
where app_id = @AppId
```

```sql
update auth_apps
set status = 0
where app_id = @AppId
```

该能力用于降低误操作风险，不能替代数据库权限控制。生产环境仍然建议使用最小权限数据库账号。

## 限制

- SQL 文件不能为空。
- 同一 Provider 作用域内不能出现重复 SQL ID。
- 不同数据库的 SQL 方言差异由各自目录维护。
