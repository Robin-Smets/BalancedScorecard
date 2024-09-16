# main.py

from services import Logger, ThreadManager
from flask import Flask
from kivy.core.window import Window

from application import Application
from data import DataStore, get_default_data_store_directory
from dashboard import DashboardService

if __name__ == '__main__':
    app = Application()
    logger = Logger(__name__)

    # configure app
    Window.size = (1280, 720)
    file_directory = get_default_data_store_directory()
    logger.log("debug", "Application configured.")

    # register services
    services = dict()
    services["DataStore"] = DataStore(file_directory)
    services["DashboardService"] = DashboardService()
    services["ThreadManager"] = ThreadManager(logger)
    app.services = services
    logger.log("debug", "Services registered.")

    # starting app
    logger.log("debug", "Initializing application...")
    app.initialize()
    logger.log("debug", "Application initialized.")
    logger.log("debug", "Starting application...")
    app.run()
