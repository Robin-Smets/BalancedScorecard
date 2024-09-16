# main.py

import logging
from services import Logger
from flask import Flask, jsonify
from kivy.core.window import Window

from application import Application
from data import DataStore, Database, get_default_data_store_directory
from dashboard import DashboardService
from threading import Thread

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
    load_data_store_thread = Thread(target=app.load_data_store)
    load_data_store_thread.daemon = True
    load_data_store_thread.start()

    # connect to live db
    connect_to_live_db_thread = Thread(target=app.connect_to_live_db)
    connect_to_live_db_thread.daemon = True
    connect_to_live_db_thread.start()

    # initialize dashboard service
    dashboard_service.dashboard_server = dashboard_server
    dashboard_service.create_dashboard()


    # run dashboard server
    run_dashboard_server_thread = Thread(target=app.run_dashboard_server)
    run_dashboard_server_thread.daemon = True
    run_dashboard_server_thread.start()

    # start app
    logger.log("debug", "Starting application...")
    app.run()
