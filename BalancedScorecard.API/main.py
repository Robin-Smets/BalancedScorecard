# app.py

"""
This script runs the application using a development server.
It contains the definition of routes and views for the application.
"""

from importlib import metadata
import os
from flask import Flask, jsonify
from flask_cors import CORS

from services.data_service import DataService
from services.thread_manager import ThreadManager

server = Flask(__name__)
services = dict()

# routes

@server.route('/')
def get():
    """Returns the API's metadata."""
    metadata = {
        "api_version": "0.1",
        "last_update": "2024-09-20",
        "documentation_url": "https://example.com/docs"
    }
    return jsonify(metadata)

# @server.route('/test/')
# def get_data_store_data():
#     """Returns the SalesOrderHeader as a CSV."""
#     table = services["DataService"].tables["SalesOrderHeader"]

#     # Generiere den CSV-Inhalt
#     def generate_csv():
#         # Header mit den Spaltennamen
#         columns = [column for column in table.columns]
#         yield ','.join(columns) + '\n'  # Headerzeile

#         # Datenzeilen
#         for row in table.rows:
#             yield ','.join([str(value) for value in row]) + '\n'  # Wertezeile

#     # Rückgabe als CSV
#     return generate_csv()

@server.route('/data_store/table_names/')
def get_data_store_table_names():
    tables = services["DataService"].tables
    table_names = [table_name for table_name in tables.keys()]
    return jsonify(table_names)

@server.route('/data_store/<table_name>/')
def get_data_store_table(table_name):
    """Returns the data of the referenced table as a CSV."""
    table = services["DataService"].tables[table_name]

    def generate_csv():
        columns = [column for column in table.columns]
        yield ';'.join(columns) + '\n'
        for row in table.rows:
            yield ';'.join([str(value) for value in row]) + '\n'

    return generate_csv()

# entry point

if __name__ == '__main__':
    # add services
    services["DataService"] = DataService()
    services["ThreadManager"] = ThreadManager()
    # load data store
    services["ThreadManager"].start_daemon_thread(services["DataService"]
                             .create_data_table_from_query, "SalesOrderHeader", "SELECT * FROM Sales.SalesOrderHeader")
    file_path = './sql_scripts/OrderVolumeCombined.sql'
    with open(file_path, 'r', encoding='utf-8') as file:
        order_volume_query = file.read()
    services["ThreadManager"].start_daemon_thread(services["DataService"]
                             .create_data_table_from_query, "OrderVolume", order_volume_query)
    # start server
    CORS(server)
    server.run(host="localhost", port=5555, debug=False)
