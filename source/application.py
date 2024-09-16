import datetime
import logging
import os
import threading
import webbrowser

from flask import Flask
from kivy.clock import Clock
from kivymd.app import MDApp
from kivy.uix.gridlayout import GridLayout
from kivymd.uix.pickers import MDDatePicker
from kivymd.uix.dialog import MDDialog
from kivymd.uix.button import MDRaisedButton, MDIconButton, MDFillRoundFlatIconButton
from kivymd.uix.boxlayout import MDBoxLayout
from kivymd.uix.menu import MDDropdownMenu
from threading import Thread
import sys
import io
from data import read_sql_from_file, Database
from crypt import encrypt_data_store, decrypt_data_store
from source.services import Logger
from view import MainFrame


class Application(MDApp):

    @property
    def services(self):
        return self._services

    @services.setter
    def services(self, value):
        self._services = value

    @property
    def dashboard_server(self):
        return self._dashboard_server

    @dashboard_server.setter
    def dashboard_server(self, value):
        self._dashboard_server = value

    def __init__(self, **kwargs):
        super(Application, self).__init__(**kwargs)
        self._dashboard = None
        self._dashboard_server = None
        self._main_frame = None
        self.services = None


    def build(self):
        self._main_frame = MainFrame()
        self.theme_cls.primary_palette = "Blue"
        self.theme_cls.theme_style = "Light"
        menu_item_texts = ['Financial Perspective', 'Customer Perspective', 'Process Perspective', 'Innovation Perspective']
        menu_items = [
            {
                "text": menu_item_text,
                "viewclass": "OneLineListItem",
                "on_release": lambda x=menu_item_text: self.menu_callback(x),
            } for menu_item_text in menu_item_texts
        ]
        self.menu = MDDropdownMenu(
            caller=self._main_frame.ids.perspective_combobox_button,
            items=menu_items,
            width_mult=4,
        )
        return self._main_frame

    def initialize(self):
        self._dashboard_server = Flask('dashboard_server')
        thread_manager = self.services["ThreadManager"]

        # load data store
        thread_manager.start_daemon_thread(self.load_data_store)
        thread_manager.start_daemon_thread(self.connect_to_live_db)

        # initialize dashboard service
        dashboard_service = self.services["DashboardService"]
        dashboard_service.create_dashboard()
        thread_manager.start_daemon_thread(self.run_dashboard_server)

    def menu_callback(self, text_item):
        print(f'{text_item} selected')

    def open_dashboard(self):
        open_dashboard_thread = Thread(target=self.open_dashboard_thread)
        open_dashboard_thread.daemon = True
        open_dashboard_thread.start()

    def update_data_store(self):
        update_data_store_thread = Thread(target=self.update_data_store_thread)
        update_data_store_thread.daemon = True
        update_data_store_thread.start()

    def update_data_store_thread(self):
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Updating data store'))
        app_path = os.getcwd()
        parent_dir = os.path.dirname(app_path)
        sql_path = f'{parent_dir}/sql/OrderVolumeCombined.sql'
        query = read_sql_from_file(sql_path)
        services = self.services
        print(services.services.keys())
        data_store = services.get_service('DataStore')
        db = data_store.databases['AdventureWorks2022']
        db.export_query_to_csv(query,f'{data_store.file_directory}/OrderVolumeCombined.csv')
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Data store updated'))

    def open_dashboard_thread(self):
        file_path = os.path.abspath("index.html")
        url = f"file://{file_path}"
        webbrowser.open(url)
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Dashboard started'))

    def run_dashboard(self):
        self.dashboard_server.run(host="127.0.0.1", port=8050, debug=False)

    def encrypt_data_store(self):
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Encrypting data store'))
        encrypt_data_store_thread = Thread(target=self.encrypt_data_store_thread)
        encrypt_data_store_thread.daemon = True
        encrypt_data_store_thread.start()

    def encrypt_data_store_thread(self):
        encrypt_data_store(key=self._main_frame.ids.crypto_key_text_field.text)
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('DataStore encrypted'))

    def decrypt_data_store(self):
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Decrypting data store'))
        decrypt_data_store_thread = Thread(target=self.decrypt_data_store_thread)
        decrypt_data_store_thread.daemon = True
        decrypt_data_store_thread.start()

    def decrypt_data_store_thread(self):
        decrypt_data_store(key=self._main_frame.ids.crypto_key_text_field.text)
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('DataStore decrypted'))

    def load_data_store(self):
        data_store = self.services["DataStore"]
        dashboard_service = self.services["DashboardService"]
        db = Database(database='AdventureWorks2022DS')
        db.import_tables_from_files('csv', directory=data_store.file_directory)
        data_store.databases[db.name] = db
        print('DataStore loaded')
        dashboard_service.dashboard_data = data_store.databases[db.name].tables
        # dashboard_service.fix_data()
        print('Daahboard data loaded')

    def connect_to_live_db(self):
        data_store = self.services["DataStore"]
        db = Database(database='AdventureWorks2022', live_db=True)
        data_store.databases[db.name] = db
        print('Live DB registered')

    def run_dashboard_server(self):
        self.dashboard_server.run(host='127.0.0.1', port=8050)
        print('Dashboard server started')
