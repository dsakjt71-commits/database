# Tyouqu.Database

`Tyouqu.Database` 是可复用的 .NET 数据库访问组件，目标是在认证中心、文件中心、小说程序等多个服务中共用同一套数据库访问能力。

## 组件源码

```text
src/Tyouqu.Database.Abstractions
src/Tyouqu.Database.Dapper
src/Tyouqu.Database.SqlServer
src/Tyouqu.Database.MySql
src/Tyouqu.Database.PostgreSql
src/Tyouqu.Database.Sqlite
src/Tyouqu.Database
```

## 当前支持

- SQL Server
- MySQL
- PostgreSQL
- SQLite
- 外置 SQL 模板
- 文件名即 SQL ID
- 单结果集查询
- 分页查询
- 多结果集查询
- 手动 reload SQL 模板

## 快速使用

统一入口（推荐应用按配置选择数据库时使用）：

```csharp
using Tyouqu.Database.Abstractions;
using Tyouqu.Database.DependencyInjection;

services.AddTyouquDatabase(options =>
{
    options.Provider = DatabaseProvider.SqlServer;
    options.ConnectionString = configuration.GetConnectionString("Default")!;
    options.SqlTemplates.RootPath = "sql";
});
```

单独 Provider 入口（只想引用某一个数据库驱动时使用）：

SQL Server:

```csharp
services.AddTyouquSqlServerDatabase(options =>
{
    options.Provider = DatabaseProvider.SqlServer;
    options.ConnectionString = configuration.GetConnectionString("Default")!;
    options.SqlTemplates.RootPath = "sql";
});
```

MySQL:

```csharp
services.AddTyouquMySqlDatabase(options =>
{
    options.Provider = DatabaseProvider.MySql;
    options.ConnectionString = configuration.GetConnectionString("Default")!;
    options.SqlTemplates.RootPath = "sql";
});
```

PostgreSQL:

```csharp
services.AddTyouquPostgreSqlDatabase(options =>
{
    options.Provider = DatabaseProvider.PostgreSql;
    options.ConnectionString = configuration.GetConnectionString("Default")!;
    options.SqlTemplates.RootPath = "sql";
});
```

SQLite:

```csharp
services.AddTyouquSqliteDatabase(options =>
{
    options.Provider = DatabaseProvider.Sqlite;
    options.ConnectionString = "Data Source=app.db";
    options.SqlTemplates.RootPath = "sql";
});
```

调用 SQL:

```csharp
var app = await db.QuerySingleOrDefaultByIdAsync<App>(
    "auth.app.getById",
    new { AppId = 1 });
```

对应文件:

```text
sql/sqlserver/auth/auth.app.getById.sql
```

四个数据库都遵循同一套 SQL 模板规则：一个 SQL ID 一个 `.sql` 文件，SQL ID 等于文件名去掉 `.sql`。例如 `auth.app.getById` 对应：

```text
sql/sqlserver/auth/auth.app.getById.sql
sql/mysql/auth/auth.app.getById.sql
sql/postgresql/auth/auth.app.getById.sql
sql/sqlite/auth/auth.app.getById.sql
```

如果 SQL 在多个数据库中完全一致，可以放到：

```text
sql/common/auth/auth.app.getById.sql
```

Provider 专用目录中的同名 SQL ID 会覆盖 `common`。

## AuthCenter 当前接入状态

认证中心在 `AppRepository:Mode` 为 `SqlTemplate` 时已经使用统一入口 `AddTyouquDatabase(...)`。默认配置仍是：

```json
{
  "Database": {
    "Provider": "SqlServer"
  }
}
```

因此当前运行行为仍是 SQL Server；切换数据库时，需要同时修改 `Database:Provider`、连接字符串和对应 Provider 目录下的 SQL 文件。

## 文档

```text
components/Tyouqu.Database/接入指南.md
components/Tyouqu.Database/SQL模板规范.md
components/Tyouqu.Database/用法示例.md
components/Tyouqu.Database/测试说明.md
components/Tyouqu.Database/目录索引.md
```

## Demo

```text
demos/Tyouqu.Database.Demo
```

## 打包

```powershell
.\components\Tyouqu.Database\pack.ps1
```

输出目录:

```text
artifacts/packages
```

## 独立构建

仓库根目录下提供了独立解决方案：

```powershell
dotnet build .\Tyouqu.Database.sln
dotnet test .\Tyouqu.Database.sln
.\components\Tyouqu.Database\pack.ps1
```

该解决方案只包含 `Tyouqu.Database.*` 组件、组件单元测试和 Demo，不依赖认证中心业务项目。后续如果需要把组件迁移到单独仓库，可以直接迁移以下目录和文件：

```text
src/Tyouqu.Database*
tests/Tyouqu.Database.UnitTests
demos/Tyouqu.Database.Demo
components/Tyouqu.Database
doc/Tyouqu.Database
Tyouqu.Database.sln
```

## Java / Python 使用方式

当前 `Tyouqu.Database` 是 .NET 类库，打包产物是 NuGet 包，Java 和 Python 不能像引用 jar 或 pip 包一样直接使用它。

Java 版组件已经放在：

```text
sdk/tyouqu-database-java
```

它是独立 Maven/JDBC 类库，复用同一套 SQL 模板目录规范和 SQL ID 规则。

如果目标是让 Java / Python 也复用同一套数据库访问能力，建议把该组件包在一个独立的数据库访问服务后面，对外提供 HTTP 或 gRPC API，然后 Java / Python 通过各自 SDK 调用服务。

如果目标只是复用 SQL 模板规范，可以为 Java / Python 分别实现原生客户端，复用 `sql/{provider}/...` 和 `sql/common/...` 的目录约定，但数据库连接、Dapper 执行器和 DI 注册逻辑需要分别重写。
