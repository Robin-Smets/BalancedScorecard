# application.py
from kivy.config import Config
Config.set('input', 'mouse', 'mouse,multitouch_on_demand')
from kivy.app import App
from kivy.uix.gridlayout import GridLayout
from source.gui.purchasing import Purchasing
from source.gui.sales import Sales

class Application(App):

    def __init__(self, **kwargs):
        super(Application, self).__init__(**kwargs)
        self._main_frame = None

    def build(self):
        self._main_frame = MainFrame()
        return self._main_frame

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