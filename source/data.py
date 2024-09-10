from sqlalchemy import create_engine
import pandas as pd
import os

def get_csv_directory():
    app_path = os.getcwd()
    parent_dir = os.path.dirname(app_path)
    return f'{parent_dir}/data'


def aggregate_by_time_unit(df, time_unit):
    # Erstelle eine neue Spalte für die Zeit-Einheit
    if time_unit == 'year':
        aggregated_df = df.groupby('OrderDateYear').agg({'TotalDue': 'sum'}).reset_index()
    elif time_unit == 'month':
        month_df = df[['OrderDateYear','OrderDateMonth', 'TimeUnitMonth', 'TotalDue']]
        aggregated_df = month_df.groupby(['OrderDateYear', 'OrderDateMonth', 'TimeUnitMonth'])[
            'TotalDue'].sum().reset_index()
        aggregated_df.rename(columns={'TotalDue': 'OrderVolume', 'TimeUnitMonth': 'TimeUnit'}, inplace=True)
    elif time_unit == 'quarter':
        aggregated_df = df.groupby('OrderDateQuarter').agg({'TotalDue': 'sum'}).reset_index()
    elif time_unit == 'cw':
        aggregated_df = df.groupby('OrderDateCalenderWeek').agg({'TotalDue': 'sum'}).reset_index()
    else:
        raise ValueError("Invalid time_unit. Choose from 'year', 'month', 'quarter', 'cw'.")

    return aggregated_df


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

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, value):
        self._name = value

    def __init__(self, servername='', database='', driver='', create_engine=False):
        if create_engine:
            if servername != '' and database != '' and driver != '':
                self._engine = create_engine(f'mssql+pyodbc://{servername}/{database}?driver={driver}')
            else:
                raise Exception("The connection parameters are missing.")

        if database != '':
            self._name = database

        self._tables = {}

    def load_tables(self, tables):
        # Update the _tables attribute
        for table in tables:
            query = f'SELECT * FROM dbo.{table}'
            self._tables[table] = pd.read_sql_query(query, self._engine)

    def import_tables_from_csv_files(self, csv_directory=''):
        if csv_directory != '':
            if not os.path.isdir(csv_directory):
                raise ValueError(f"Das Verzeichnis '{csv_directory}' existiert nicht.")
        else:
            csv_directory = get_csv_directory()

        # Iterieren über alle Dateien im angegebenen Verzeichnis
        for filename in os.listdir(csv_directory):
            # Überprüfen, ob die Datei eine CSV-Datei ist
            if filename.endswith('.csv'):
                # Erstellen des vollständigen Pfads zur Datei
                file_path = os.path.join(csv_directory, filename)

                # Einlesen der CSV-Datei in einen DataFrame
                df = pd.read_csv(file_path, sep=';', engine='python')

                # Entfernen der '.csv'-Erweiterung vom Dateinamen und als Schlüssel verwenden
                table_name = filename[:-4]  # Entfernen der letzten 4 Zeichen ('.csv')

                # Hinzufügen des DataFrames zum Dictionary
                self.tables[table_name] = df

        return self.tables

class DatabaseService:
    _instance = None

    @property
    def databases(self):
        return self._databases

    @databases.setter
    def databases(self, value):
        self._databases = value

    @property
    def data_directory(self):
        return self._data_directory

    @data_directory.setter
    def data_directory(self, value):
        self._data_directory = value

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = super(DatabaseService, cls).__new__(cls, *args, **kwargs)
        return cls._instance

    def __init__(self):
        # Initialisierung wird nur einmal durchgeführt
        if not hasattr(self, '_initialized'):
            self._initialized = True
            self._data_directory = get_csv_directory()
            self._databases = {}