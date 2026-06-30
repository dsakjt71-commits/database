using System.Text;

namespace Tyouqu.Database.Abstractions;

public static class SqlSafetyAnalyzer
{
    public static string Normalize(string sql)
    {
        var builder = new StringBuilder(sql.Length);
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var inLineComment = false;
        var inBlockComment = false;

        for (var i = 0; i < sql.Length; i++)
        {
            var current = sql[i];
            var next = i + 1 < sql.Length ? sql[i + 1] : '\0';

            if (inLineComment)
            {
                if (current is '\r' or '\n')
                {
                    inLineComment = false;
                    builder.Append(' ');
                }
                continue;
            }

            if (inBlockComment)
            {
                if (current == '*' && next == '/')
                {
                    inBlockComment = false;
                    i++;
                    builder.Append(' ');
                }
                continue;
            }

            if (!inSingleQuote && !inDoubleQuote && current == '-' && next == '-')
            {
                inLineComment = true;
                i++;
                continue;
            }

            if (!inSingleQuote && !inDoubleQuote && current == '/' && next == '*')
            {
                inBlockComment = true;
                i++;
                continue;
            }

            if (!inDoubleQuote && current == '\'')
            {
                inSingleQuote = !inSingleQuote;
                builder.Append(' ');
                continue;
            }

            if (!inSingleQuote && current == '"')
            {
                inDoubleQuote = !inDoubleQuote;
                builder.Append(' ');
                continue;
            }

            builder.Append(inSingleQuote || inDoubleQuote ? ' ' : char.ToLowerInvariant(current));
        }

        return string.Join(' ', builder.ToString().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool IsFullTableUpdate(string normalizedSql)
    {
        return normalizedSql.StartsWith("update ", StringComparison.Ordinal) &&
               !ContainsWhereClause(normalizedSql);
    }

    public static bool IsFullTableDelete(string normalizedSql)
    {
        return (normalizedSql.StartsWith("delete from ", StringComparison.Ordinal) ||
                normalizedSql == "delete" ||
                normalizedSql.StartsWith("delete ", StringComparison.Ordinal)) &&
               !ContainsWhereClause(normalizedSql);
    }

    private static bool ContainsWhereClause(string normalizedSql)
    {
        return normalizedSql.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("where");
    }
}
