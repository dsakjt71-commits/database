namespace Tyouqu.Database.Abstractions;

public class TyouquDatabaseException : Exception
{
    public TyouquDatabaseException(string message)
        : base(message)
    {
    }

    public TyouquDatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public string? SqlId { get; set; }

    public string? Provider { get; set; }
}
