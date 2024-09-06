from kivy.uix.boxlayout import BoxLayout
from kivy.uix.widget import Widget
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
from matplotlib.backends.backend_agg import FigureCanvasAgg as FigureCanvas
import os
from kivy.lang import Builder
import numpy as np
from kivy.uix.image import Image
from kivy.graphics.texture import Texture
import io
from kivy_garden.matplotlib.backend_kivyagg import FigureCanvasKivyAgg
from kivy.clock import Clock
from source.gui.plot import PlotCanvas
from source.data import DatabaseService
current_dir = os.path.dirname(__file__)
kv_file_path = os.path.join(current_dir, 'sales.kv')
Builder.load_file(kv_file_path)
from matplotlib.gridspec import GridSpec

class Sales(BoxLayout):
    def __init__(self, **kwargs):
        super(Sales, self).__init__(**kwargs)

    def draw_complex_plot(self):
        plot_canvas = self.ids.sales_plot_canvas
        df = sns.load_dataset('iris')
        databases = DatabaseService().databases
        databases['DataWarehouse'].load_tables(['RevenuePerMonth'])
        data_warehouse = DatabaseService().databases['DataWarehouse']

        revenue_per_month = data_warehouse.tables['RevenuePerMonth']

        def complex_plot(fig, data, **kwargs):
            # Erstelle ein GridSpec Layout
            gs = GridSpec(3, 3, height_ratios=[1, 1, 2], width_ratios=[2, 1, 1])

            # Hauptachse für Histogramm
            ax1 = fig.add_subplot(gs[2, :])
            sns.barplot(x=revenue_per_month['Month'], y=revenue_per_month['Revenue'], ax=ax1, palette='Blues_d')
            ax1.set_title('Revenue Per Month')
            ax1.set_xlabel('Month')
            ax1.set_ylabel('Revenue')
            ax1.set_xticklabels(ax1.get_xticklabels(), rotation=45)

            # Subplots für Piecharts
            pie_data = [data['sepal_length'].mean(), data['sepal_width'].mean(),
                        data['petal_length'].mean(), data['petal_width'].mean()]
            titles = ['Sepal Length', 'Sepal Width', 'Petal Length', 'Petal Width']

            count = 1
            for i in range(2):
                for j in range(2):
                    ax = fig.add_subplot(gs[i, j])
                    ax.pie(pie_data, labels=titles, autopct='%1.1f%%', startangle=90)
                    ax.set_title(f'Pie Chart {count}')
                    count += 1

        plot_canvas.dataframe = df
        plot_canvas.args = {'plot_func': complex_plot}
        plot_canvas.update_plot()

    def draw_scatterplot(self):
        # Example for scatter plot
        plot_canvas = self.ids.sales_plot_canvas
        df = sns.load_dataset('iris')

        # Define the plotting function
        def scatter_plot(fig, data, **kwargs):
            ax = fig.add_subplot(111)
            ax.scatter(data['sepal_length'], data['sepal_width'])
            ax.set_title('Scatter Plot')

        plot_canvas.dataframe = df
        plot_canvas.args = {'plot_func': scatter_plot}
        plot_canvas.update_plot()

    def draw_histogram(self):
        # Example for histogram
        plot_canvas = self.ids.sales_plot_canvas
        df = sns.load_dataset('iris')

        # Define the plotting function
        def histogram_plot(fig, data, **kwargs):
            ax = fig.add_subplot(111)
            ax.hist(data['sepal_length'], bins=20, color='orange')
            ax.set_title('Histogram')

        plot_canvas.dataframe = df
        plot_canvas.args = {'plot_func': histogram_plot}
        plot_canvas.update_plot()