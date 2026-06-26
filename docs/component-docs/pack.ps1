param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "artifacts/packages"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
$output = Join-Path $repoRoot $OutputPath
New-Item -ItemType Directory -Path $output -Force | Out-Null

$projects = @(
    "src\Tyouqu.Database.Abstractions\Tyouqu.Database.Abstractions.csproj",
    "src\Tyouqu.Database.Dapper\Tyouqu.Database.Dapper.csproj",
    "src\Tyouqu.Database.SqlServer\Tyouqu.Database.SqlServer.csproj",
    "src\Tyouqu.Database.MySql\Tyouqu.Database.MySql.csproj",
    "src\Tyouqu.Database.PostgreSql\Tyouqu.Database.PostgreSql.csproj",
    "src\Tyouqu.Database.Sqlite\Tyouqu.Database.Sqlite.csproj",
    "src\Tyouqu.Database\Tyouqu.Database.csproj"
)

foreach ($project in $projects) {
    dotnet pack (Join-Path $repoRoot $project) -c $Configuration -o $output
}

Write-Host "Packages written to $output"
