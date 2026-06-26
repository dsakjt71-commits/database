# Docker 部署与 SQL 维护

## 推荐方式

生产环境不要把 SQL 文件只放在镜像内部，推荐把 SQL 目录挂载到宿主机：

```yaml
volumes:
  - D:/AuthService/data/sql:/app/data/sql
```

服务配置：

```yaml
environment:
  AppRepository__Mode: "SqlTemplate"
  Database__SqlTemplates__RootPath: "/app/data/sql"
  Database__SqlTemplates__ReloadMode: "Manual"
  Database__SqlTemplates__EnableHotReload: "false"
  InternalApi__ApiKey: "${AUTHCENTER_INTERNAL_API_KEY}"
```

这样 SQL 文件实际保存在服务器本地：

```text
D:\AuthService\data\sql
```

容器内读取路径：

```text
/app/data/sql
```

## 首次部署

首次部署时，把项目里的 SQL 模板复制到服务器：

```powershell
Copy-Item -Path D:\AuthService\app\sql -Destination D:\AuthService\data\sql -Recurse -Force
```

或者从代码仓库复制：

```powershell
Copy-Item -Path src\AuthCenter.API\sql -Destination D:\AuthService\data\sql -Recurse -Force
```

也可以直接使用仓库里的初始化脚本：

```powershell
.\deploy\init-sql-data.ps1
```

目录结构应类似：

```text
D:\AuthService\data\sql
  sqlserver
    auth
      app.sql
```

## 修改 SQL

修改服务器上的文件：

```text
D:\AuthService\data\sql\sqlserver\auth\app.sql
```

当前版本使用手动加载模式。修改 SQL 后，推荐调用 reload 接口重新加载，不需要重启容器：

```http
POST /api/internal/database/sql-templates/reload
Authorization: Bearer {access_token}
X-Internal-Api-Key: {internal_api_key}
```

调用成功后服务会重新加载 `/app/data/sql` 下的 SQL 模板。

如果 reload 失败，接口会返回错误，旧的 SQL 模板仍保留在内存中。

## 为什么不建议生产自动热加载

生产环境 SQL 文件如果保存错误，自动热加载会马上影响线上请求。推荐流程是：

1. 备份当前 SQL 目录。
2. 修改 SQL 文件。
3. 调用 reload 接口。
4. 验证核心接口。
5. 如果异常，恢复备份并再次重启。

## 回滚

备份示例：

```powershell
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
Copy-Item D:\AuthService\data\sql D:\AuthService\data\sql_backup\$stamp -Recurse
```

回滚示例：

```powershell
Remove-Item D:\AuthService\data\sql -Recurse -Force
Copy-Item D:\AuthService\data\sql_backup\20260604_120000 D:\AuthService\data\sql -Recurse
Invoke-RestMethod `
  -Uri http://192.168.1.12:5044/api/internal/database/sql-templates/reload `
  -Method Post `
  -Headers @{
    Authorization = "Bearer {access_token}"
    "X-Internal-Api-Key" = "{internal_api_key}"
  }
```
