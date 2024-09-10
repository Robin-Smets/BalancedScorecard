from kivy.uix.boxlayout import BoxLayout
import seaborn as sns
import os
from kivy.lang import Builder
from source.data import DatabaseService
from source.data import aggregate_by_time_unit
current_dir = os.path.dirname(__file__)
kv_file_path = os.path.join(current_dir, 'sales.kv')
Builder.load_file(kv_file_path)
from matplotlib.gridspec import GridSpec
from source.gui.plot import PlotCanvas
from kivy.uix.modalview import ModalView
from kivymd.uix.spinner import MDSpinner
import pandas as pd

class Sales(BoxLayout):
    def __init__(self, **kwargs):
        super(Sales, self).__init__(**kwargs)

    def get_data(self):
        data_warehouse = DatabaseService().databases['AdventureWorks2022']
        sales_order_header_frame = data_warehouse.tables['Sales.SalesOrderHeader']
        sales_order_header_frame_subset = sales_order_header_frame[[
            'OrderDateCalenderWeek',
            'OrderDateMonth',
            'OrderDateQuarter',
            'OrderDateYear',
            'TimeUnitCalenderWeek',
            'TimeUnitMonth',
            'TimeUnitQuarter',
            'TotalDue']
        ]
        sales_order_header_frame_subset.loc[:, 'TotalDue'] = sales_order_header_frame_subset['TotalDue'].str.replace(',', '.')
        sales_order_header_frame_subset.loc[:, 'TotalDue'] = pd.to_numeric(sales_order_header_frame_subset['TotalDue'],
                                                                     errors='coerce')
        aggregated_order_volume = aggregate_by_time_unit(sales_order_header_frame_subset, 'month')
        return aggregated_order_volume

    def draw_order_volume_plot(self):
        plot_canvas = self.ids.sales_plot_canvas
        # self.show_spinner()

        aggregated_order_volume = self.get_data()

        plot_canvas = self.ids.sales_plot_canvas

        def scatter_plot(fig, data, **kwargs):
            ax = fig.add_subplot(111)
            ax.bar(data['TimeUnit'], data['OrderVolume'])
            ax.set_title('Order volume over time')
            ax.set_xlabel('Month')
            ax.set_ylabel('Order volume')
            ax.set_xticklabels(data['TimeUnit'],rotation=90)

        plot_canvas.dataframe = aggregated_order_volume
        plot_canvas.args = {'plot_func': scatter_plot}
        plot_canvas.update_plot()

        # def complex_plot(fig, data, **kwargs):
        #     # Erstelle ein GridSpec Layout
        #     gs = GridSpec(3, 3, height_ratios=[1, 1, 2], width_ratios=[2, 1, 1])
        #
        #     # # Hauptachse für Histogramm
        #     # ax1 = fig.add_subplot(gs[2, :])
        #     # sns.barplot(x=aggregated_order_volume['TimeUnit'], y=aggregated_order_volume['OrderVolume'], ax=ax1, palette='Blues_d')
        #     # ax1.set_title('Revenue Per Month')
        #     # ax1.set_xlabel('Month')
        #     # ax1.set_ylabel('Revenue')
        #     #
        #     # # Subplots für Piecharts
        #     pie_data = [data['sepal_length'].mean(), data['sepal_width'].mean(),
        #                  data['petal_length'].mean(), data['petal_width'].mean()]
        #     titles = ['Sepal Length', 'Sepal Width', 'Petal Length', 'Petal Width']
        #
        #     count = 1
        #     for i in range(2):
        #         for j in range(2):
        #             ax = fig.add_subplot(gs[i, j])
        #             ax.pie(pie_data, labels=titles, autopct='%1.1f%%', startangle=90)
        #             ax.set_title(f'Pie Chart {count}')
        #             count += 1
        #
        # plot_canvas.dataframe = df
        # plot_canvas.args = {'plot_func': complex_plot}
        # plot_canvas.update_plot()

        # self.hide_spinner()

    def draw_scatter_plot(self):
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


    def show_spinner(self):
        self.ids.sales_plot_canvas.clear_widgets()
        self.ids.sales_plot_canvas.spinner = ModalView(size_hint=(0.2, 0.2))
        self.ids.sales_plot_canvas.spinner.add_widget(MDSpinner())
        self.ids.sales_plot_canvas.spinner.open()

    def hide_spinner(self):
        self.ids.sales_plot_canvas.spinner.dismiss()