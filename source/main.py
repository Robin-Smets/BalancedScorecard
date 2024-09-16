# main.py
import logging
from logging import Logger
from services import Logger

from flask import Flask, jsonify

from application import Application
from data import DataStore, Database, get_default_data_store_directory
from dashboard import DashboardService
from threading import Thread
from kivy.core.window import Window

def load_data_store():
    db = Database(database='AdventureWorks2022DS')
    db.import_tables_from_files('csv', directory=file_directory)
    data_store.databases[db.name] = db
    print('DataStore loaded')
    dashboard_service.dashboard_data = data_store.databases[db.name].tables
    # dashboard_service.fix_data()
    print('Daahboard data loaded')

def connect_to_live_db():
    db = Database(database='AdventureWorks2022', live_db=True)
    data_store.databases[db.name] = db
    print('Live DB registered')

def run_dashboard_server():
    dashboard_server.run(host='127.0.0.1', port=8050)
    print('Dashboard server started')

if __name__ == '__main__':
    logging.basicConfig(level=logging.DEBUG)  # Setzt den Log-Level auf DEBUG
    logger = Logger(__name__)
    # configure app
    Window.size = (1280, 720)
    file_directory = get_default_data_store_directory()
    logger.log("debug", "Application configured.")

    # create objects
    services = dict()
    dashboard_service = DashboardService()
    data_store = DataStore(file_directory)
    app = Application()
    dashboard_server = Flask('dashboard_server')
    logger.log("debug", "Objects created.")

    # register services
    services["DataStore"] = data_store
    services["DashboardService"] = dashboard_service
    app.services = services
    logger.log("debug", "Services registered.")

    # load data store
    load_data_store_thread = Thread(target=load_data_store)
    load_data_store_thread.daemon = True
    load_data_store_thread.start()

    # connect to live db
    connect_to_live_db_thread = Thread(target=connect_to_live_db)
    connect_to_live_db_thread.daemon = True
    connect_to_live_db_thread.start()

    # initialize dashboard service
    dashboard_service.dashboard_server = dashboard_server
    dashboard_service.create_dashboard()


    # run dashboard server
    run_dashboard_server_thread = Thread(target=run_dashboard_server)
    run_dashboard_server_thread.daemon = True
    run_dashboard_server_thread.start()

    # start app
    logger.log("debug", "Starting application...")
    app.run()
