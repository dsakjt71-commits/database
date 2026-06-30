from abc import ABC, abstractmethod
from pathlib import Path
from typing import Optional

from .exceptions import TyouquDatabaseException
from .models import DatabaseOptions


class SqlTemplateStore(ABC):
    @abstractmethod
    def get_required_sql(self, sql_id: str) -> str:
        raise NotImplementedError

    @abstractmethod
    def try_get_sql(self, sql_id: str) -> Optional[str]:
        raise NotImplementedError

    @abstractmethod
    def reload(self) -> None:
        raise NotImplementedError


class FileSqlTemplateStore(SqlTemplateStore):
    def __init__(self, options: DatabaseOptions):
        self._options = options
        self._templates: dict[str, str] = {}
        self.reload()

    def get_required_sql(self, sql_id: str) -> str:
        sql = self.try_get_sql(sql_id)
        if sql is None:
            raise TyouquDatabaseException(
                f"SQL template was not found. SqlId={sql_id}",
                sql_id=sql_id,
                provider=self._options.provider.name,
            )
        return sql

    def try_get_sql(self, sql_id: str) -> Optional[str]:
        if sql_id is None:
            return None
        return self._templates.get(sql_id.lower())

    def reload(self) -> None:
        root_path = self._resolve_root_path(self._options.sql_templates.root_path)
        if not root_path.is_dir():
            raise TyouquDatabaseException(f"SQL template root path does not exist. Path={root_path}")

        loaded: dict[str, str] = {}
        self._load_directory(root_path / "common", loaded, allow_override=False)
        self._load_directory(root_path / self._options.provider.directory_name, loaded, allow_override=True)
        self._templates = dict(loaded)

    def _load_directory(self, directory: Path, loaded: dict[str, str], *, allow_override: bool) -> None:
        if not directory.is_dir():
            return

        current_scope_ids = set()
        for file in sorted(directory.rglob("*.sql")):
            if not file.is_file():
                continue
            sql_id, sql = self._parse_file(file)
            key = sql_id.lower()
            if key in current_scope_ids and self._options.sql_templates.fail_on_duplicate_sql_id:
                raise TyouquDatabaseException(
                    f"Duplicate SQL template id was found in the same scope. SqlId={sql_id}, File={file}"
                )
            current_scope_ids.add(key)
            if not allow_override and key in loaded and self._options.sql_templates.fail_on_duplicate_sql_id:
                raise TyouquDatabaseException(f"Duplicate SQL template id was found. SqlId={sql_id}, File={file}")
            loaded[key] = sql

    @staticmethod
    def _parse_file(file: Path) -> tuple[str, str]:
        try:
            sql = file.read_text(encoding="utf-8").strip()
        except OSError as ex:
            raise TyouquDatabaseException(f"Failed to read SQL template file. File={file}", ex) from ex
        if not sql:
            raise TyouquDatabaseException(f"SQL template file is empty. File={file}")
        if "-- @id " in sql.lower():
            raise TyouquDatabaseException(f"SQL template file must use filename as sql id. Remove -- @id marker. File={file}")
        return file.name[: -len(".sql")], sql

    @staticmethod
    def _resolve_root_path(root_path: str) -> Path:
        path = Path(root_path)
        return path if path.is_absolute() else (Path.cwd() / path).resolve()
