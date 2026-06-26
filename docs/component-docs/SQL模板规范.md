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

## 限制

- SQL 文件不能为空。
- 同一 Provider 作用域内不能出现重复 SQL ID。
- 不同数据库的 SQL 方言差异由各自目录维护。
