from kivy.uix.boxlayout import BoxLayout
from kivy.lang import Builder
import os

current_dir = os.path.dirname(__file__)
kv_file_path = os.path.join(current_dir, 'purchasing.kv')
Builder.load_file(kv_file_path)

class Purchasing(BoxLayout):
    def __init__(self, **kwargs):
        super(Purchasing, self).__init__(**kwargs)
