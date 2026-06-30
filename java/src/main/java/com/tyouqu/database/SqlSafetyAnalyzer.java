package com.tyouqu.database;

import java.util.Arrays;
import java.util.Locale;

public final class SqlSafetyAnalyzer {
    private SqlSafetyAnalyzer() {
    }

    public static String normalize(String sql) {
        StringBuilder builder = new StringBuilder(sql.length());
        boolean inSingleQuote = false;
        boolean inDoubleQuote = false;
        boolean inLineComment = false;
        boolean inBlockComment = false;

        for (int i = 0; i < sql.length(); i++) {
            char current = sql.charAt(i);
            char next = i + 1 < sql.length() ? sql.charAt(i + 1) : '\0';

            if (inLineComment) {
                if (current == '\r' || current == '\n') {
                    inLineComment = false;
                    builder.append(' ');
                }
                continue;
            }
            if (inBlockComment) {
                if (current == '*' && next == '/') {
                    inBlockComment = false;
                    i++;
                    builder.append(' ');
                }
                continue;
            }
            if (!inSingleQuote && !inDoubleQuote && current == '-' && next == '-') {
                inLineComment = true;
                i++;
                continue;
            }
            if (!inSingleQuote && !inDoubleQuote && current == '/' && next == '*') {
                inBlockComment = true;
                i++;
                continue;
            }
            if (!inDoubleQuote && current == '\'') {
                inSingleQuote = !inSingleQuote;
                builder.append(' ');
                continue;
            }
            if (!inSingleQuote && current == '"') {
                inDoubleQuote = !inDoubleQuote;
                builder.append(' ');
                continue;
            }

            builder.append(inSingleQuote || inDoubleQuote ? ' ' : Character.toLowerCase(current));
        }

        return String.join(" ", builder.toString().trim().split("\\s+"));
    }

    public static boolean isFullTableUpdate(String normalizedSql) {
        return normalizedSql.startsWith("update ") && !containsWhereClause(normalizedSql);
    }

    public static boolean isFullTableDelete(String normalizedSql) {
        return (normalizedSql.startsWith("delete from ") || normalizedSql.equals("delete") || normalizedSql.startsWith("delete "))
            && !containsWhereClause(normalizedSql);
    }

    private static boolean containsWhereClause(String normalizedSql) {
        return Arrays.asList(normalizedSql.toLowerCase(Locale.ROOT).split("\\s+")).contains("where");
    }
}
