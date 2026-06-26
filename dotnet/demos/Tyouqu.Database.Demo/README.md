# Tyouqu.Database Demo

该 Demo 用于验证 `Tyouqu.Database` 通用数据库组件的第一版能力：

- SQL Server Provider
- Dapper 执行器
- 外置 SQL 模板加载
- 通过 SQL ID 执行查询
- 简单分页查询

## 运行前准备

设置数据库连接字符串环境变量：

```powershell
$env:TYOUQU_DB_CONNECTION='Server=192.168.1.2;Database=AuthManagement;User Id=sa;Password=******;TrustServerCertificate=True;Encrypt=True;'
```

## 运行 Demo

```powershell
dotnet run --project demos\Tyouqu.Database.Demo\Tyouqu.Database.Demo.csproj
```

## SQL 模板位置

```text
demos/Tyouqu.Database.Demo/sql/sqlserver/demo/app.sql
```

Demo 里通过 SQL ID 调用 SQL，SQL ID 等于文件名去掉 `.sql`：

```text
demo.app.count
demo.app.list
demo.app.page
demo.app.detailWithStats
```

业务代码不直接写 SQL，真实 SQL 放在外部 `.sql` 文件中。
