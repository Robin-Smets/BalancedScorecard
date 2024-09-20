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

@server.route('/test/')
def get_sales_order_header():
    """Returns the SalesOrderHeader as a CSV."""
    table = services["DataService"].tables["SalesOrderHeader"]

    # Generiere den CSV-Inhalt
    def generate_csv():
        # Header mit den Spaltennamen
        columns = [column for column in table.columns]
        yield ','.join(columns) + '\n'  # Headerzeile

        # Datenzeilen
        for row in table.rows:
            yield ','.join([str(value) for value in row]) + '\n'  # Wertezeile

    # Rückgabe als CSV
    return generate_csv()

# entry point

if __name__ == '__main__':
    # add services
    services["DataService"] = DataService()
    services["ThreadManager"] = ThreadManager()
    # initialize services
    services["ThreadManager"].start_daemon_thread(services["DataService"]
                             .create_data_table_from_query, "SalesOrderHeader", "SELECT * FROM Sales.SalesOrderHeader")
    
    # start server
    CORS(server)
    server.run(host="localhost", port=5555, debug=False)
