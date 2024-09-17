from datetime import datetime

from kivy.uix.gridlayout import GridLayout
from kivymd.uix.button import MDRaisedButton
from kivymd.uix.dialog import MDDialog
from kivymd.uix.menu import MDDropdownMenu
from kivymd.uix.pickers import MDDatePicker


class MainFrame(GridLayout):
    def open_date_picker(self, date_type):
        date_dialog = MDDatePicker()
        date_dialog.bind(on_save=lambda instance, value, date_range: self.set_date(date_type, value))
        date_dialog.open()

    def set_date(self, date_type, date_value):
        if date_type == 'from':
            self.ids.from_date_button.text = str(date_value)
        elif date_type == 'until':
            self.ids.until_date_button.text = str(date_value)

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
        log_text += f'[{datetime.now().strftime("%d.%m.%Y - %H:%M:%S")}] {message}\n'
        self.ids.console_text_field.text = log_text

def create_dropdown_menu(menu_item_texts, menu_callback, caller):
    menu_items = [
        {
            "text": menu_item_text,
            "viewclass": "OneLineListItem",
            "on_release": lambda x=menu_item_text: menu_callback(x),
        } for menu_item_text in menu_item_texts
    ]
    menu = MDDropdownMenu(
        caller=caller,
        items=menu_items,
        width_mult=4,
    )
    return menu