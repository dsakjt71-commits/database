# database

`database` is a lightweight SQL-template database access component with .NET and Java implementations.

The two implementations share the same SQL template convention:

```text
sql/common/auth/auth.user.getById.sql
sql/mysql/auth/auth.user.getById.sql
sql/sqlserver/auth/auth.user.getById.sql
sql/postgresql/auth/auth.user.getById.sql
sql/sqlite/auth/auth.user.getById.sql
```

Provider-specific templates override `common` templates with the same SQL id. The SQL id is the file name without `.sql`, for example `auth.user.getById`.

## Layout

```text
dotnet/   .NET implementation, tests, and demo
java/     Java JDBC implementation and tests
docs/     Existing design and component documents
```

## .NET

Build and test:

```powershell
cd dotnet
dotnet build .\database.sln
dotnet test .\database.sln --no-build
```

Pack NuGet packages:

```powershell
cd dotnet
.\pack.ps1
```

## Java

Build and test:

```powershell
cd java
mvn test
```

Install to the local Maven repository:

```powershell
cd java
mvn install
```

Then use it from another Maven project:

```xml
<dependency>
    <groupId>database</groupId>
    <artifactId>database</artifactId>
    <version>1.0.0</version>
</dependency>
```

See `java/README.md` for JDBC driver and Spring Boot examples.
