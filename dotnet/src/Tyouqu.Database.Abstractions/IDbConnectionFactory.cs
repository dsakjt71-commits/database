using System.Data;

namespace Tyouqu.Database.Abstractions;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}

