# data_service.py

import pyodbc
from data.data_table import DataTable

class DataService:

    @property
    def tables(self):
        return self._tables

    def __init__(self): 
        self._conn_str = (  # TODO: make configurable
            "DRIVER={ODBC Driver 18 for SQL Server};"
            "SERVER=localhost;"
            "DATABASE=AdventureWorks2022;"
            "UID=sa;"
            "PWD=@Sql42;"
            "TrustServerCertificate=yes;"
        )
        self._tables = dict()

    def create_data_table_from_query(self, table_name, query):
        try:
            # execute query
            connection = pyodbc.connect(self._conn_str)
            cursor = connection.cursor()
            cursor.execute(query)
            # create data table
            data_table = DataTable()
            data_table.table_name = table_name
            data_table.columns = [column[0] for column in cursor.description]
            data_table.rows = cursor.fetchall()
            self.tables[data_table.table_name] = data_table
            # close connection
            cursor.close()
            connection.close()
        except Exception as e:
            print(e)
            return None
