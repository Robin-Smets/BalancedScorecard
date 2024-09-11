from sqlalchemy import create_engine
import pandas as pd
import os
from pathlib import Path
from services import ServiceProvider


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

    #  TODO: make more generic
    # def load_tables(self, tables):
    #     # Update the _tables attribute
    #     for table in tables:
    #         query = f'SELECT * FROM dbo.{table}'
    #         self._tables[table] = pd.read_sql_query(query, self._engine)

    def import_tables_from_files(self, file_extension, directory=''):
        if directory == '':
            directory = ServiceProvider().get_service('DataStore').file_directory
        for filename in os.listdir(directory):
            if filename.endswith(f'.{file_extension}'):
                file_path = os.path.join(directory, filename)
                if file_extension == 'csv':
                    df = pd.read_csv(file_path, sep=';', engine='python')
                    table_name = Path(file_path).stem
                    self.tables[table_name] = df


class DataStore:
    @property
    def databases(self):
        return self._databases

    @databases.setter
    def databases(self, value):
        self._databases = value

    @property
    def file_directory(self):
        return self._file_directory

    @file_directory.setter
    def file_directory(self, value):
        self._file_directory = value

    def __init__(self, file_directory=''):
        if file_directory == '':
            self._file_directory = get_default_data_store_directory()
        else:
            self._file_directory = file_directory
        self._databases = {}

def get_default_data_store_directory():
    app_path = os.getcwd()
    parent_dir = os.path.dirname(app_path)
    return f'{parent_dir}/data'

def aggregate_by_time_unit(df, time_unit):
    # Erstelle eine neue Spalte für die Zeit-Einheit
    if time_unit == 'year':
        year_df = df[['OrderDateYear', 'TimeUnitYear', 'TotalDue']]
        aggregated_df = year_df.groupby(['OrderDateYear', 'TimeUnitYear'])[
            'TotalDue'].sum().reset_index()
        aggregated_df.rename(columns={'TotalDue': 'OrderVolume', 'TimeUnitYear': 'TimeUnit'}, inplace=True)

    elif time_unit == 'month':
        month_df = df[['OrderDateYear','OrderDateMonth', 'TimeUnitMonth', 'TotalDue']]
        aggregated_df = month_df.groupby(['OrderDateYear', 'OrderDateMonth', 'TimeUnitMonth'])[
            'TotalDue'].sum().reset_index()
        aggregated_df.rename(columns={'TotalDue': 'OrderVolume', 'TimeUnitMonth': 'TimeUnit'}, inplace=True)

    elif time_unit == 'quarter':
        quarter_df = df[['OrderDateYear','OrderDateQuarter', 'TimeUnitQuarter', 'TotalDue']]
        aggregated_df = quarter_df.groupby(['OrderDateYear', 'OrderDateQuarter', 'TimeUnitQuarter'])[
            'TotalDue'].sum().reset_index()
        aggregated_df.rename(columns={'TotalDue': 'OrderVolume', 'TimeUnitQuarter': 'TimeUnit'}, inplace=True)

    elif time_unit == 'cw':
        cw_df = df[['OrderDateYear','OrderDateCalenderWeek', 'TimeUnitCalenderWeek', 'TotalDue']]
        aggregated_df = cw_df.groupby(['OrderDateYear', 'OrderDateCalenderWeek', 'TimeUnitCalenderWeek'])[
            'TotalDue'].sum().reset_index()
        aggregated_df.rename(columns={'TotalDue': 'OrderVolume', 'TimeUnitCalenderWeek': 'TimeUnit'}, inplace=True)

    else:
        raise ValueError("Invalid time_unit. Choose from 'year', 'month', 'quarter', 'cw'.")

    return aggregated_df


def aggregate_by_column(df, column_name):
    """
    Aggregates data in the DataFrame by the specified column and calculates the sum of 'TotalDue'.

    Parameters:
        df (pd.DataFrame): The DataFrame containing the data.
        column_name (str): The name of the column to aggregate by.

    Returns:
        pd.DataFrame: A DataFrame with the aggregated sums of 'TotalDue' over the specified column.
    """
    if column_name not in df.columns:
        raise ValueError(f"Column '{column_name}' does not exist in the dataframe.")

    # Group by the specified column and calculate the sum of 'TotalDue'
    aggregated_df = df[[column_name, 'TotalDue']].groupby(column_name)['TotalDue'].sum().reset_index()

    # Rename the 'TotalDue' column to 'OrderVolume'
    aggregated_df.rename(columns={'TotalDue': 'OrderVolume'}, inplace=True)

    return aggregated_df