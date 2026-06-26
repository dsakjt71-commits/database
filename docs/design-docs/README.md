# Tyouqu.Database 通用数据库组件文档

## 目录说明

本文档目录用于描述 `Tyouqu.Database` 通用数据库访问组件的设计、使用、接入、配置、功能范围和维护规范。该组件的目标是让认证中心、文件中心、小说程序以及后续其他服务复用同一套数据库访问能力，避免每个项目重复封装连接、事务、分页、SQL 模板和多数据库适配逻辑。

## 文档列表

| 文档 | 说明 |
| --- | --- |
| [组件设计文档](./组件设计文档.md) | 说明组件定位、架构分层、核心接口和扩展方式 |
| [使用文档](./使用文档.md) | 面向业务开发人员，说明如何执行查询、分页、事务和调用外置 SQL |
| [接入文档](./接入文档.md) | 面向项目接入人员，说明如何在认证中心或其他服务中引入组件 |
| [配置文档](./配置文档.md) | 说明数据库 Provider、连接字符串、SQL 模板、热加载和日志配置 |
| [功能文档](./功能文档.md) | 说明组件第一版、第二版和后续版本规划的功能边界 |
| [维护文档](./维护文档.md) | 说明 SQL 模板维护、版本发布、线上变更、回滚和排障规范 |
| [Docker部署与SQL维护](./Docker部署与SQL维护.md) | 说明 Docker 下 SQL 文件挂载、reload 和回滚 |

## 对外复用入口

如果要把组件拿到其他项目使用，优先查看：

```text
components/Tyouqu.Database
```

## 核心目标

`Tyouqu.Database` 不是认证中心专用组件，也不是某个业务系统的 Repository。它是一个通用数据库基础组件，主要负责：

- 统一数据库连接创建和释放
- 统一参数化查询和命令执行
- 统一事务管理
- 统一分页模型
- 支持 SQL Server、MySQL、PostgreSQL、SQLite 等不同数据库
- 支持外置 SQL 模板，避免 SQL 写死在代码中
- 支持按数据库 Provider 加载不同 SQL
- 支持可控热加载或手动重新加载 SQL 模板
- 为业务 Repository 提供稳定的底层能力

## 非目标

组件不直接处理具体业务逻辑，不应该知道用户、应用、Token、订单、文件、小说章节等业务概念。

业务服务仍然应该保留自己的 Repository，例如：

```text
AuthCenter.UserRepository
FileCenter.FileRepository
NovelCenter.BookRepository
```

这些 Repository 可以依赖 `Tyouqu.Database`，但业务规则不放进 `Tyouqu.Database`。

## 推荐落地顺序

1. 实现 `Tyouqu.Database.Abstractions` 和 `Tyouqu.Database.Dapper`。
2. 实现 SQL Server Provider 和 SQL Server 方言。
3. 在认证中心中新增一两个 Repository 使用该组件验证可行性。
4. 增加 MySQL、PostgreSQL、SQLite Provider。
5. 逐步把认证中心现有存储过程访问迁移到外置 SQL 模板。
