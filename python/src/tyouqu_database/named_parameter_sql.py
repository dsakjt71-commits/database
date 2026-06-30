from dataclasses import dataclass


@dataclass(frozen=True)
class ParsedSql:
    sql: str
    parameter_names: list[str]


def parse_named_parameter_sql(sql: str) -> ParsedSql:
    parsed = []
    names = []
    in_single_quote = False
    in_double_quote = False
    i = 0

    while i < len(sql):
        current = sql[i]
        if current == "'" and not in_double_quote:
            in_single_quote = not in_single_quote
            parsed.append(current)
            i += 1
            continue
        if current == '"' and not in_single_quote:
            in_double_quote = not in_double_quote
            parsed.append(current)
            i += 1
            continue
        if current == ":" and i + 1 < len(sql) and sql[i + 1] == ":":
            parsed.append("::")
            i += 2
            continue
        if (
            current == ":"
            and not in_single_quote
            and not in_double_quote
            and i + 1 < len(sql)
            and _is_name_start(sql[i + 1])
        ):
            start = i + 1
            end = start + 1
            while end < len(sql) and _is_name_part(sql[end]):
                end += 1
            names.append(sql[start:end])
            parsed.append("?")
            i = end
            continue
        parsed.append(current)
        i += 1

    return ParsedSql("".join(parsed), names)


def _is_name_start(value: str) -> bool:
    return value.isalpha() or value == "_"


def _is_name_part(value: str) -> bool:
    return value.isalnum() or value == "_"
