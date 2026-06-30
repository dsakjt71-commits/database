from tyouqu_database import parse_named_parameter_sql


def test_parse_named_parameters_skips_quotes_and_postgresql_cast():
    parsed = parse_named_parameter_sql("select ':skip', col::int from users where id = :id and name = :name")

    assert parsed.sql == "select ':skip', col::int from users where id = ? and name = ?"
    assert parsed.parameter_names == ["id", "name"]
