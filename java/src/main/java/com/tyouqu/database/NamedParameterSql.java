package com.tyouqu.database;

import java.util.ArrayList;
import java.util.List;

final class NamedParameterSql {
    private NamedParameterSql() {
    }

    static ParsedSql parse(String sql) {
        StringBuilder jdbcSql = new StringBuilder(sql.length());
        List<String> names = new ArrayList<>();
        boolean inSingleQuote = false;
        boolean inDoubleQuote = false;

        for (int i = 0; i < sql.length(); i++) {
            char current = sql.charAt(i);
            if (current == '\'' && !inDoubleQuote) {
                inSingleQuote = !inSingleQuote;
                jdbcSql.append(current);
                continue;
            }
            if (current == '"' && !inSingleQuote) {
                inDoubleQuote = !inDoubleQuote;
                jdbcSql.append(current);
                continue;
            }
            if (current == ':' && i + 1 < sql.length() && sql.charAt(i + 1) == ':') {
                jdbcSql.append("::");
                i++;
                continue;
            }
            if (current == ':' && !inSingleQuote && !inDoubleQuote && i + 1 < sql.length() && isNameStart(sql.charAt(i + 1))) {
                int start = i + 1;
                int end = start + 1;
                while (end < sql.length() && isNamePart(sql.charAt(end))) {
                    end++;
                }
                names.add(sql.substring(start, end));
                jdbcSql.append('?');
                i = end - 1;
                continue;
            }
            jdbcSql.append(current);
        }
        return new ParsedSql(jdbcSql.toString(), List.copyOf(names));
    }

    private static boolean isNameStart(char value) {
        return Character.isLetter(value) || value == '_';
    }

    private static boolean isNamePart(char value) {
        return Character.isLetterOrDigit(value) || value == '_';
    }

    record ParsedSql(String jdbcSql, List<String> parameterNames) {
    }
}
