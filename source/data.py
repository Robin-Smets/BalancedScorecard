from sqlalchemy import create_engine
import pandas as pd

class Database:
    @property
    def engine(self):
        return self._engine

    @engine.setter
    def engine(self, value):
        self._engine = value

    @property
    def tables(self):
        return self._tables

    @tables.setter
    def tables(self, value):
        self._tables = value

    def __init__(self, servername, database, driver):
        self._engine = create_engine(f'mssql+pyodbc://{servername}/{database}?driver={driver}')
        self._tables = {}

    def load_tables(self, tables):
        # Update the _tables attribute
        for table in tables:
            query = f'SELECT * FROM dbo.{table}'
            self._tables[table] = pd.read_sql_query(query, self._engine)

class DatabaseService:
    _instance = None

    @property
    def databases(self):
        return self._databases

    @databases.setter
    def databases(self, value):
        self._databases = value

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = super(DatabaseService, cls).__new__(cls, *args, **kwargs)
        return cls._instance

    def __init__(self):
        # Initialisierung wird nur einmal durchgeführt
        if not hasattr(self, '_initialized'):
            self._initialized = True
            self._databases = {}