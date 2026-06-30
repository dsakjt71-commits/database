class TyouquDatabaseException(RuntimeError):
    def __init__(self, message, cause=None, *, sql_id=None, provider=None):
        super().__init__(message)
        self.cause = cause
        self.sql_id = sql_id
        self.provider = provider
