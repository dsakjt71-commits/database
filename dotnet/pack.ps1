param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "..\artifacts\packages"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot\.."
$output = Join-Path $PSScriptRoot $OutputPath
New-Item -ItemType Directory -Path $output -Force | Out-Null

$projects = @(
    "dotnet\src\Tyouqu.Database.Abstractions\Tyouqu.Database.Abstractions.csproj",
    "dotnet\src\Tyouqu.Database.Dapper\Tyouqu.Database.Dapper.csproj",
    "dotnet\src\Tyouqu.Database.SqlServer\Tyouqu.Database.SqlServer.csproj",
    "dotnet\src\Tyouqu.Database.MySql\Tyouqu.Database.MySql.csproj",
    "dotnet\src\Tyouqu.Database.PostgreSql\Tyouqu.Database.PostgreSql.csproj",
    "dotnet\src\Tyouqu.Database.Sqlite\Tyouqu.Database.Sqlite.csproj",
    "dotnet\src\Tyouqu.Database\Tyouqu.Database.csproj"
)

foreach ($project in $projects) {
    dotnet pack (Join-Path $repoRoot $project) -c $Configuration -o $output
}

Write-Host "Packages written to $output"
