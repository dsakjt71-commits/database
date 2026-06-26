namespace Tyouqu.Database.Abstractions;

public interface ISqlTemplateStore
{
    string GetRequiredSql(string sqlId);

    bool TryGetSql(string sqlId, out string sql);

    Task ReloadAsync(CancellationToken cancellationToken = default);
}

