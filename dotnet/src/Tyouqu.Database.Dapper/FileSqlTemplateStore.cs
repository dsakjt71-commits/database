using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tyouqu.Database.Abstractions;

namespace Tyouqu.Database.Dapper;

public sealed class FileSqlTemplateStore : ISqlTemplateStore
{
    private readonly DatabaseOptions _options;
    private readonly ILogger<FileSqlTemplateStore> _logger;
    private IReadOnlyDictionary<string, string> _templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public FileSqlTemplateStore(IOptions<DatabaseOptions> options, ILogger<FileSqlTemplateStore> logger)
    {
        _options = options.Value;
        _logger = logger;
        ReloadAsync().GetAwaiter().GetResult();
    }

    public string GetRequiredSql(string sqlId)
    {
        if (TryGetSql(sqlId, out var sql))
        {
            return sql;
        }

        throw new TyouquDatabaseException($"SQL template was not found. SqlId={sqlId}")
        {
            SqlId = sqlId,
            Provider = _options.Provider.ToString()
        };
    }

    public bool TryGetSql(string sqlId, out string sql)
    {
        return _templates.TryGetValue(sqlId, out sql!);
    }

    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        var rootPath = ResolveRootPath(_options.SqlTemplates.RootPath);
        if (!Directory.Exists(rootPath))
        {
            throw new TyouquDatabaseException($"SQL template root path does not exist. Path={rootPath}");
        }

        var templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        LoadDirectory(Path.Combine(rootPath, "common"), templates, allowOverride: false);
        LoadDirectory(Path.Combine(rootPath, _options.Provider.ToString().ToLowerInvariant()), templates, allowOverride: true);

        _templates = templates;
        _logger.LogInformation("Loaded {Count} SQL templates from {RootPath}. Provider={Provider}", templates.Count, rootPath, _options.Provider);
        return Task.CompletedTask;
    }

    private void LoadDirectory(string directory, IDictionary<string, string> templates, bool allowOverride)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        var currentScopeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in Directory.EnumerateFiles(directory, "*.sql", SearchOption.AllDirectories))
        {
            var template = ParseFile(file);
            if (!currentScopeIds.Add(template.SqlId) && _options.SqlTemplates.FailOnDuplicateSqlId)
            {
                throw new TyouquDatabaseException($"Duplicate SQL template id was found in the same scope. SqlId={template.SqlId}, File={file}");
            }

            if (!allowOverride && templates.ContainsKey(template.SqlId) && _options.SqlTemplates.FailOnDuplicateSqlId)
            {
                throw new TyouquDatabaseException($"Duplicate SQL template id was found. SqlId={template.SqlId}, File={file}");
            }

            templates[template.SqlId] = template.Sql;
        }
    }

    private static SqlTemplate ParseFile(string file)
    {
        var sql = File.ReadAllText(file).Trim();
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new TyouquDatabaseException($"SQL template file is empty. File={file}");
        }

        if (sql.Contains("-- @id ", StringComparison.OrdinalIgnoreCase))
        {
            throw new TyouquDatabaseException($"SQL template file must use filename as sql id. Remove -- @id marker. File={file}");
        }

        var sqlId = Path.GetFileNameWithoutExtension(file);
        return new SqlTemplate(sqlId, sql);
    }

    private static string ResolveRootPath(string rootPath)
    {
        return Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, rootPath));
    }

    private sealed record SqlTemplate(string SqlId, string Sql);
}
