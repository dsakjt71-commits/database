import pytest

from tyouqu_database import (
    DatabaseOptions,
    DatabaseProvider,
    FileSqlTemplateStore,
    SqlTemplateOptions,
    TyouquDatabaseException,
)


def test_provider_sql_overrides_common(tmp_path):
    (tmp_path / "common").mkdir()
    (tmp_path / "sqlite").mkdir()
    (tmp_path / "common" / "auth.user.get.sql").write_text("select 'common'", encoding="utf-8")
    (tmp_path / "sqlite" / "auth.user.get.sql").write_text("select 'sqlite'", encoding="utf-8")

    store = FileSqlTemplateStore(
        DatabaseOptions(
            provider=DatabaseProvider.SQLITE,
            sql_templates=SqlTemplateOptions(root_path=str(tmp_path)),
        )
    )

    assert store.get_required_sql("AUTH.USER.GET") == "select 'sqlite'"


def test_empty_sql_file_fails(tmp_path):
    (tmp_path / "common").mkdir()
    (tmp_path / "common" / "empty.sql").write_text("  ", encoding="utf-8")

    with pytest.raises(TyouquDatabaseException, match="empty"):
        FileSqlTemplateStore(DatabaseOptions(sql_templates=SqlTemplateOptions(root_path=str(tmp_path))))


def test_legacy_id_marker_fails(tmp_path):
    (tmp_path / "common").mkdir()
    (tmp_path / "common" / "legacy.sql").write_text("-- @id legacy\nselect 1", encoding="utf-8")

    with pytest.raises(TyouquDatabaseException, match="@id"):
        FileSqlTemplateStore(DatabaseOptions(sql_templates=SqlTemplateOptions(root_path=str(tmp_path))))
