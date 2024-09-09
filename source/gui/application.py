from kivymd.app import MDApp
from kivy.uix.gridlayout import GridLayout
from kivymd.uix.pickers import MDDatePicker
from kivymd.uix.dialog import MDDialog
from kivymd.uix.button import MDRaisedButton
from kivymd.uix.boxlayout import MDBoxLayout
from kivymd.uix.menu import MDDropdownMenu
from source.gui.purchasing import Purchasing
from source.gui.sales import Sales

class Application(MDApp):

    def __init__(self, **kwargs):
        super(Application, self).__init__(**kwargs)
        self._main_frame = None


    def build(self):
        self._main_frame = MainFrame()
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

class MainFrame(GridLayout):

    def open_sales(self):
        self.ids.module_container.clear_widgets()
        self.ids.module_container.add_widget(Sales())

    def open_purchasing(self):
        self.ids.module_container.clear_widgets()
        self.ids.module_container.add_widget(Purchasing())

    def call_method_on_child(self, method_name):
        """Call a method of the current child in module_container with the given method name."""
        if self.ids.module_container.children:
            current_widget = self.ids.module_container.children[0]
            if hasattr(current_widget, method_name):
                method = getattr(current_widget, method_name)
                if callable(method):
                    method()
                else:
                    print(f'{method_name} is not callable in {current_widget}')
            else:
                print(f'{current_widget} has no method {method_name}')

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
