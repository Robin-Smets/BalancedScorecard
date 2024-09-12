# main.py
from flask import Flask, jsonify

from application import Application, AppService
from services import ServiceProvider
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
    dashboard_service.fix_data()
    print('Daahboard data loaded')

def run_dashboard_server():
    dashboard_server.run(host='127.0.0.1', port=8050)
    print('Dashboard server started')

if __name__ == '__main__':
    # configure app
    Window.size = (1280, 720)
    file_directory = get_default_data_store_directory()
    print('Application configured')

    # create objects
    services = ServiceProvider()
    app_service = AppService()
    dashboard_service = DashboardService()
    data_store = DataStore(file_directory)
    app = Application()
    dashboard_server = Flask('dashboard_server')
    print('Objects created')

    # register services
    services.register_service('DataStore', data_store)
    services.register_service('AppService', app_service)
    services.register_service('DashboardService', dashboard_service)
    print('Service registered')

    # load data store
    load_data_store_thread = Thread(target=load_data_store)
    load_data_store_thread.daemon = True
    load_data_store_thread.start()
    print('Thread started: load_data_store_thread')

    # set up api routes
    @dashboard_server.route("/api/data")
    def get_api_data():
        return jsonify({"message": "Hallo von der Flask API", "value": 123})

    # initialize app service
    app_service.app = app
    print('Service initialized: app_service')

    # initialize dashboard service
    dashboard_service.dashboard_server = dashboard_server
    dashboard_service.create_dashboard()


    # run dashboard server
    run_dashboard_server_thread = Thread(target=run_dashboard_server)
    run_dashboard_server_thread.daemon = True
    run_dashboard_server_thread.start()
    print('Thread started: run_dashboard_server_thread')

    # start app
    app.run()
    print('Application started')
