# database Java

Java/JDBC version of `database`. It reuses the same SQL template convention:

```text
sql/common/auth/auth.app.getById.sql
sql/mysql/auth/auth.app.getById.sql
sql/sqlserver/auth/auth.app.getById.sql
sql/postgresql/auth/auth.app.getById.sql
sql/sqlite/auth/auth.app.getById.sql
```

Provider-specific templates override `common` templates with the same SQL id.

## Build

```powershell
cd java
mvn test
mvn package
```

## Install Locally

After installing this library into your local Maven repository:

```powershell
cd D:\CurSor\AuthService\DB\database\java
D:\Tools\apache-maven-3.9.16\bin\mvn.cmd install
```

Other Maven projects can reference it with:

```xml
<dependency>
    <groupId>database</groupId>
    <artifactId>database</artifactId>
    <version>1.0.0</version>
</dependency>
```

If your other project uses the same custom Maven settings and local repository, make sure IDEA/Maven also uses:

```text
Maven home path: D:\Tools\apache-maven-3.9.16
User settings file: D:\Tools\apache-maven-3.9.16\conf\settings.xml
Local repository: D:\Tools\apache-maven-3.9.16\repository
```

## JDBC Drivers

`database` does not bundle database drivers. Add the driver required by your project.

MySQL:

```xml
<dependency>
    <groupId>com.mysql</groupId>
    <artifactId>mysql-connector-j</artifactId>
    <version>8.4.0</version>
</dependency>
```

SQL Server:

```xml
<dependency>
    <groupId>com.microsoft.sqlserver</groupId>
    <artifactId>mssql-jdbc</artifactId>
    <version>12.8.1.jre11</version>
</dependency>
```

PostgreSQL:

```xml
<dependency>
    <groupId>org.postgresql</groupId>
    <artifactId>postgresql</artifactId>
    <version>42.7.4</version>
</dependency>
```

SQLite:

```xml
<dependency>
    <groupId>org.xerial</groupId>
    <artifactId>sqlite-jdbc</artifactId>
    <version>3.46.1.0</version>
</dependency>
```

## Usage

```java
DatabaseOptions options = DatabaseOptions.builder()
    .provider(DatabaseProvider.MYSQL)
    .sqlTemplates(SqlTemplateOptions.builder().rootPath("sql").build())
    .build();

SqlTemplateStore store = new FileSqlTemplateStore(options);
DbExecutor db = new JdbcDbExecutor(dataSource);
SqlTemplateExecutor executor = new SqlTemplateExecutor(db, store, Dialects.forProvider(options.provider()));

User user = executor.querySingleOrDefaultById(
    "auth.user.getById",
    Map.of("userId", 1),
    User.class
);
```

SQL templates use named parameters:

```sql
select * from users where id = :userId
```

Place SQL files in the consuming project, for example:

```text
your-java-project
  sql
    mysql
      auth
        auth.user.getById.sql
    common
      auth
        auth.user.exists.sql
```

Then set:

```java
SqlTemplateOptions.builder().rootPath("sql").build()
```

## Spring Boot Example

In a Spring Boot project, reuse Spring's `DataSource`:

```java
@Configuration
public class TyouquDatabaseConfig {
    @Bean
    SqlTemplateExecutor sqlTemplateExecutor(DataSource dataSource) {
        DatabaseOptions options = DatabaseOptions.builder()
            .provider(DatabaseProvider.MYSQL)
            .sqlTemplates(SqlTemplateOptions.builder().rootPath("sql").build())
            .build();

        SqlTemplateStore store = new FileSqlTemplateStore(options);
        DbExecutor db = new JdbcDbExecutor(dataSource);
        return new SqlTemplateExecutor(db, store, Dialects.forProvider(options.provider()));
    }
}
```

Use it in a service:

```java
@Service
public class UserQueryService {
    private final SqlTemplateExecutor sql;

    public UserQueryService(SqlTemplateExecutor sql) {
        this.sql = sql;
    }

    public User findById(long userId) {
        return sql.querySingleOrDefaultById(
            "auth.user.getById",
            Map.of("userId", userId),
            User.class
        );
    }
}
```
