from kivy.app import App
from kivy.uix.boxlayout import BoxLayout
from kivy.uix.button import Button
from kivy.uix.screenmanager import ScreenManager, Screen
from kivy.uix.gridlayout import GridLayout
from kivy.uix.widget import Widget


class MainMenu(Screen):
    pass


class Module1(Screen):
    pass


class Module2(Screen):
    pass


class ModuleContainer(BoxLayout):
    def __init__(self, **kwargs):
        super(ModuleContainer, self).__init__(**kwargs)
        self.orientation = 'horizontal'
        self.add_widget(NavigationPanel())
        self.add_widget(ScreenManagerWidget())


class NavigationPanel(BoxLayout):
    def __init__(self, **kwargs):
        super(NavigationPanel, self).__init__(**kwargs)
        self.orientation = 'vertical'
        self.add_widget(Button(text="Module 1", on_press=self.change_to_module1))
        self.add_widget(Button(text="Module 2", on_press=self.change_to_module2))

    def change_to_module1(self, instance):
        app = App.get_running_app()
        app.screen_manager.current = 'module1'

    def change_to_module2(self, instance):
        app = App.get_running_app()
        app.screen_manager.current = 'module2'


class ScreenManagerWidget(ScreenManager):
    def __init__(self, **kwargs):
        super(ScreenManagerWidget, self).__init__(**kwargs)
        self.add_widget(MainMenu(name='main'))
        self.add_widget(Module1(name='module1'))
        self.add_widget(Module2(name='module2'))


class App(App):
    def build(self):
        self.screen_manager = ScreenManagerWidget()
        return ModuleContainer()


