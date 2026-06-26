package com.tyouqu.database;

import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

class NamedParameterSqlTest {
    @Test
    void parsesNamedParametersOutsideQuotedText() {
        NamedParameterSql.ParsedSql parsed = NamedParameterSql.parse(
            "select ':ignored', id from users where id = :id and name = :name"
        );

        assertEquals("select ':ignored', id from users where id = ? and name = ?", parsed.jdbcSql());
        assertEquals(List.of("id", "name"), parsed.parameterNames());
    }

    @Test
    void preservesPostgreSqlCastSyntax() {
        NamedParameterSql.ParsedSql parsed = NamedParameterSql.parse(
            "select created_at::text from users where id = :id"
        );

        assertEquals("select created_at::text from users where id = ?", parsed.jdbcSql());
        assertEquals(List.of("id"), parsed.parameterNames());
    }
}
