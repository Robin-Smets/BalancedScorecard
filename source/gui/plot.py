from kivy.app import App
from kivy.uix.boxlayout import BoxLayout
from kivy.clock import Clock
from kivy_garden.matplotlib.backend_kivyagg import FigureCanvasKivyAgg
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import numpy as np

class PlotCanvas(BoxLayout):

    @property
    def figure(self):
        return self._figure

    @figure.setter
    def figure(self, value):
        self._figure = value

    @property
    def plot_canvas(self):
        return self._plot_canvas

    @plot_canvas.setter
    def plot_canvas(self, value):
        self._plot_canvas = value

    @property
    def event(self):
        return self._event

    @event.setter
    def event(self, value):
        self._event = value

    @property
    def dataframe(self):
        return self._dataframe

    @dataframe.setter
    def dataframe(self, value):
        self._dataframe = value

    @property
    def args(self):
        return self._args

    @args.setter
    def args(self, value):
        self._args = value

    def __init__(self, **kwargs):
        super(PlotCanvas, self).__init__(**kwargs)

        self._figure = plt.figure()
        self._plot_canvas = FigureCanvasKivyAgg(self.figure)
        self.add_widget(self._plot_canvas)
        self._event = None
        self._dataframe = pd.DataFrame()
        self._args = {}

    def schedule_update(self, interval):
        self.event = Clock.schedule_interval(self.update_plot, interval)

    def update_plot(self):
        """Clears the figure and redraws the plot using the provided plot function in args."""
        if 'plot_func' in self.args:
            # Clear the figure
            self.figure.clear()

            # Call the plotting function with provided arguments
            self.args['plot_func'](self.figure, self.dataframe, **self.args)

            # Update the canvas
            self.plot_canvas.draw()


        # Aktualisiere die Figur
        self.plot_canvas.draw()