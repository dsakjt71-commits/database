from dataclasses import dataclass

from .models import DatabaseProvider


def _trim(sql: str) -> str:
    return sql.strip().removesuffix(";")


@dataclass(frozen=True)
class SqlDialect:
    name: str
    parameter_prefix: str
    quote_start: str
    quote_end: str
    quote_escape: str
    page_style: str

    def quote_identifier(self, identifier: str) -> str:
        escaped = identifier.replace(self.quote_end, self.quote_escape)
        return f"{self.quote_start}{escaped}{self.quote_end}"

    def build_paged_sql(self, sql: str, offset: int, page_size: int) -> str:
        if self.page_style == "sqlserver":
            return f"{_trim(sql)}\noffset {offset} rows fetch next {page_size} rows only"
        return f"{_trim(sql)}\nlimit {page_size} offset {offset}"


def dialect_for_provider(provider: DatabaseProvider) -> SqlDialect:
    if isinstance(provider, str):
        provider = DatabaseProvider(provider.lower())
    if provider == DatabaseProvider.SQL_SERVER:
        return SqlDialect(provider.name, ":", "[", "]", "]]", "sqlserver")
    if provider == DatabaseProvider.MYSQL:
        return SqlDialect(provider.name, ":", "`", "`", "``", "limit")
    if provider in (DatabaseProvider.POSTGRESQL, DatabaseProvider.SQLITE):
        return SqlDialect(provider.name, ":", '"', '"', '""', "limit")
    raise ValueError(f"Unsupported provider: {provider}")
