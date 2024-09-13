import datetime
import os
import threading
import webbrowser
from kivy.clock import Clock
from kivymd.app import MDApp
from kivy.uix.gridlayout import GridLayout
from kivymd.uix.pickers import MDDatePicker
from kivymd.uix.dialog import MDDialog
from kivymd.uix.button import MDRaisedButton, MDIconButton
from kivymd.uix.boxlayout import MDBoxLayout
from kivymd.uix.menu import MDDropdownMenu
from threading import Thread
import sys
import io

class AppService:
    @property
    def app(self):
        return self._app

    @app.setter
    def app(self, value):
        self._app = value

    @property
    def dashboard(self):
        return self._dashboard

    @dashboard.setter
    def dashboard(self, value):
        self._dashboard = value

    @property
    def dashboard_server(self):
        return self._dashboard_server

    @dashboard_server.setter
    def dashboard_server(self, value):
        self._dashboard_server = value

    def __init__(self):
        self._app = None
        self._dashboard = None
        self._dashboard_server = None


class Application(MDApp):

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


    def build(self):
        self._main_frame = MainFrame()
        self.theme_cls.primary_palette = "Blue"
        self.theme_cls.theme_style = "Light"
        menu_items = [
            {
                "text": f"Item {i}",
                "viewclass": "OneLineListItem",
                "on_release": lambda x=f"Item {i}": self.menu_callback(x),
            } for i in range(5)
        ]
        self.menu = MDDropdownMenu(
            caller=self._main_frame.ids.time_unit_button,
            items=menu_items,
            width_mult=4,
        )
        return self._main_frame

    def menu_callback(self, text_item):
        print(text_item)

    def open_dashboard(self):
        open_dashboard_thread = Thread(target=self.open_dashboard_thread)
        open_dashboard_thread.daemon = True
        open_dashboard_thread.start()

    def update_data_store(self):
        update_data_store_thread = Thread(target=self.update_data_store_thread)
        update_data_store_thread.daemon = True
        update_data_store_thread.start()

    def update_data_store_thread(self):
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Data store updated'))

    def open_dashboard_thread(self):
        file_path = os.path.abspath("index.html")
        url = f"file://{file_path}"
        webbrowser.open(url)
        Clock.schedule_once(lambda dt: self._main_frame.append_user_log('Dashboard started'))

    def run_dashboard(self):
        self.dashboard_server.run(host="127.0.0.1", port=8050, debug=False)


class MainFrame(GridLayout):
    def open_date_picker(self, date_type):
        date_dialog = MDDatePicker()
        date_dialog.bind(on_save=lambda instance, value, date_range: self.set_date(date_type, value))
        date_dialog.open()

    def set_date(self, date_type, date_value):
        if date_type == 'start':
            self.ids.start_date_button.text = str(date_value)
        elif date_type == 'end':
            self.ids.end_date_button.text = str(date_value)

    def open_options_dialog(self):
        self.dialog = MDDialog(
            title="Options",
            text="Hier können verschiedene Programmeinstellungen vorgenommen werden.",
            buttons=[
                MDRaisedButton(
                    text="OK",
                    on_release=lambda x: self.dialog.dismiss()
                ),
                MDRaisedButton(
                    text="Cancel",
                    on_release=lambda x: self.dialog.dismiss()
                ),
            ],
        )
        self.dialog.open()

    def open_login_dialog(self):
        pass

    def exit(self):
        pass

    def append_user_log(self, message):
        log_text = self.ids.console_text_field.text
        log_text += f'[{datetime.datetime.now().strftime("%d.%m.%Y - %H:%M:%S")}] {message}\n'
        self.ids.console_text_field.text = log_text
