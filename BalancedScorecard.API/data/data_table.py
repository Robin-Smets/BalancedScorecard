# data_table.py

class DataTable:

    @property
    def table_name(self):
        return self._table_name
    @table_name.setter
    def table_name(self, value):
        self._table_name = value

    @property
    def columns(self):
        return self._columns
    @columns.setter
    def columns(self, value):
        self._columns = value

    @property
    def rows(self):
        return self._rows
    @rows.setter
    def rows(self, value):
        self._rows = value

    def __init__(self):
        self._table_name = ""
        self._rows = []
        self._columns = []
        

    

