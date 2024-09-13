# data.py

import pyodbc
import numpy as np
import pandas as pd
import dask.dataframe as dd
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

    def __init__(self, servername='', database='', driver='', live_db=False):
        if live_db:
            self._conn_str = (
                "DRIVER={ODBC Driver 18 for SQL Server};"
                "SERVER=localhost;"  # oder IP-Adresse des Servers
                "DATABASE=AdventureWorks2022;"
                "UID=sa;"
                "PWD=@Splitsoul3141;"
                "TrustServerCertificate=yes;"
            )

        if database != '':
            self._name = database

        self._tables = {}

    def execute_query(self, query):
        try:
            connection = pyodbc.connect(self._conn_str)
            cursor = connection.cursor()
            cursor.execute(query)
            rows = cursor.fetchall()
            cursor.close()
            connection.close()
            return rows
        except Exception as e:
            print(e)
            return None

    def create_dataframe_from_query(self, query):
        try:
            with (self.engine.connect() as connection):
                result = pd.read_sql_query(query, connection)
                return result
        except Exception as e:
            print(e)


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

# def aggregate_by_time_unit(df, time_unit):
#     # Erstelle eine neue Spalte für die Zeit-Einheit
#     if time_unit == 'year':
#         year_df = df[['OrderDateYear', 'TimeUnitYear', 'OrderVolume']]
#         aggregated_df = year_df.groupby(['OrderDateYear', 'TimeUnitYear'])[
#             'OrderVolume'].sum().reset_index()
#         aggregated_df.rename(columns={'TimeUnitYear': 'TimeUnit'}, inplace=True)
#
#     elif time_unit == 'month':
#         month_df = df[['OrderDateYear','OrderDateMonth', 'TimeUnitMonth', 'OrderVolume']]
#         aggregated_df = month_df.groupby(['OrderDateYear', 'OrderDateMonth', 'TimeUnitMonth'])[
#             'OrderVolume'].sum().reset_index()
#         aggregated_df.rename(columns={'TimeUnitMonth': 'TimeUnit'}, inplace=True)
#
#     elif time_unit == 'quarter':
#         quarter_df = df[['OrderDateYear','OrderDateQuarter', 'TimeUnitQuarter', 'OrderVolume']]
#         aggregated_df = quarter_df.groupby(['OrderDateYear', 'OrderDateQuarter', 'TimeUnitQuarter'])[
#             'OrderVolume'].sum().reset_index()
#         aggregated_df.rename(columns={'TimeUnitQuarter': 'TimeUnit'}, inplace=True)
#
#     elif time_unit == 'cw':
#         cw_df = df[['OrderDateYear','OrderDateCalenderWeek', 'TimeUnitCalenderWeek', 'OrderVolume']]
#         aggregated_df = cw_df.groupby(['OrderDateYear', 'OrderDateCalenderWeek', 'TimeUnitCalenderWeek'])[
#             'OrderVolume'].sum().reset_index()
#         aggregated_df.rename(columns={'TimeUnitCalenderWeek': 'TimeUnit'}, inplace=True)
#
#     else:
#         raise ValueError("Invalid time_unit. Choose from 'year', 'month', 'quarter', 'cw'.")
#
#     return aggregated_df


def aggregate_by_time_unit(df, time_unit):
    # Check if the input dataframe is a pandas dataframe, convert to dask if necessary
    is_pandas = isinstance(df, pd.DataFrame)
    if is_pandas:
        df = dd.from_pandas(df, npartitions=4)  # Adjust npartitions based on data size

    # Select and aggregate based on the time_unit
    if time_unit == 'year':
        year_df = df[['OrderDateYear', 'TimeUnitYear', 'OrderVolume']]
        aggregated_df = year_df.groupby(['OrderDateYear', 'TimeUnitYear'])['OrderVolume'].sum().reset_index()
        aggregated_df = aggregated_df.rename(columns={'TimeUnitYear': 'TimeUnit'})

    elif time_unit == 'month':
        month_df = df[['OrderDateYear', 'OrderDateMonth', 'TimeUnitMonth', 'OrderVolume']]
        aggregated_df = month_df.groupby(['OrderDateYear', 'OrderDateMonth', 'TimeUnitMonth'])[
            'OrderVolume'].sum().reset_index()
        aggregated_df = aggregated_df.rename(columns={'TimeUnitMonth': 'TimeUnit'})

    elif time_unit == 'quarter':
        quarter_df = df[['OrderDateYear', 'OrderDateQuarter', 'TimeUnitQuarter', 'OrderVolume']]
        aggregated_df = quarter_df.groupby(['OrderDateYear', 'OrderDateQuarter', 'TimeUnitQuarter'])[
            'OrderVolume'].sum().reset_index()
        aggregated_df = aggregated_df.rename(columns={'TimeUnitQuarter': 'TimeUnit'})

    elif time_unit == 'cw':
        cw_df = df[['OrderDateYear', 'OrderDateCalenderWeek', 'TimeUnitCalenderWeek', 'OrderVolume']]
        aggregated_df = cw_df.groupby(['OrderDateYear', 'OrderDateCalenderWeek', 'TimeUnitCalenderWeek'])[
            'OrderVolume'].sum().reset_index()
        aggregated_df = aggregated_df.rename(columns={'TimeUnitCalenderWeek': 'TimeUnit'})

    else:
        raise ValueError("Invalid time_unit. Choose from 'year', 'month', 'quarter', 'cw'.")

    # Convert the result back to a pandas dataframe if the input was pandas
    if is_pandas:
        aggregated_df = aggregated_df.compute()

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

def aggregate_by_column_with_percentage(df, id_column, name_column, sum_column):
    """
    Aggregates data in the DataFrame by the specified column (e.g., CustomerID), calculates the sum of 'TotalDue' for each group,
    and computes the percentage of each group's sum relative to the total sum, while keeping 'CustomerName' intact.

    Parameters:
        df (pd.DataFrame or dd.DataFrame): The DataFrame containing the data. Can be a Dask DataFrame for large datasets.
        column_name (str): The name of the column to aggregate by (e.g., CustomerID).
        sum_column (str): The name of the column containing the values to sum (e.g., OrderVolume).

    Returns:
        pd.DataFrame: A DataFrame with the aggregated sums, 'CustomerName', and their percentages over the specified column.
    """
    if isinstance(df, pd.DataFrame):
        # Convert Pandas DataFrame to Dask DataFrame for large datasets
        df = dd.from_pandas(df, npartitions=100)  # Adjust npartitions based on your data size

    if id_column not in df.columns:
        raise ValueError(f"Column '{id_column}' does not exist in the dataframe.")

    if sum_column not in df.columns:
        raise ValueError(f"Column '{sum_column}' does not exist in the dataframe.")

    if name_column not in df.columns:
        raise ValueError(f"Column {name_column} does not exist in the dataframe.")

    # Convert sum_column to numeric (e.g., OrderVolume) - handles possible comma as decimal separator
    df[sum_column] = df[sum_column].map(convert_to_float, meta=('x', 'float64'))

    # Group by the specified column (e.g., CustomerID) and calculate the sum, keeping 'CustomerName'
    aggregated_df = df[[id_column, name_column, sum_column]].groupby([id_column, name_column])[sum_column].sum().reset_index()

    # Only keep the top 10 based on the sum_column (e.g., OrderVolume)
    aggregated_df = aggregated_df.nlargest(10, sum_column)

    # Rename the sum column to 'OrderVolume'
    aggregated_df = aggregated_df.rename(columns={sum_column: 'OrderVolume'})

    # Ensure the 'OrderVolume' is numeric
    aggregated_df['OrderVolume'] = dd.to_numeric(aggregated_df['OrderVolume'], errors='coerce')

    # Compute the total sum of 'OrderVolume'
    total_sum = aggregated_df['OrderVolume'].sum().compute()

    # Convert the Dask DataFrame to a Pandas DataFrame for percentage calculation
    aggregated_df = aggregated_df.compute()

    # Calculate percentages based on the total sum
    aggregated_df['Percentage'] = (aggregated_df['OrderVolume'] / total_sum) * 100

    return aggregated_df

def convert_to_float(x):
    try:
        if isinstance(x, str):
            return float(x.replace(',', '.'))
        else:
            return x
    except ValueError:
        return np.nan  # Oder 0, je nach gewünschter Fehlerbehandlung
