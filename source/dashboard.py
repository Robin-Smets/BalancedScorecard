import queue
import threading

import dash
import pandas as pd
import plotly.express as px
import plotly.graph_objects as go
from dash import html, dcc
import dash_bootstrap_components as dbc
from dash.dependencies import Input, Output
from data import aggregate_by_time_unit, aggregate_by_column_with_percentage
from source.services import ServiceProvider


class DashboardService:
    @property
    def dashboard_server(self):
        return self._dashboard_server

    @dashboard_server.setter
    def dashboard_server(self, value):
        self._dashboard_server = value

    @property
    def dashboard_data(self):
        return self._dashboard_data

    @dashboard_data.setter
    def dashboard_data(self, value):
        self._dashboard_data = value

    @property
    def dashboard(self):
        return self._dashboard

    @dashboard.setter
    def dashboard(self, value):
        self._dashboard = value

    def __init__(self):
        self._dashboard_server = None
        self._dashboard_data = None
        self._dashboard = None
        self._order_volume_over_time_data_queue = queue.Queue()
        self._order_volume_over_customer_data_queue = queue.Queue()
        self._order_volume_over_product_data_queue = queue.Queue()
        self._order_volume_over_territory_data_queue = queue.Queue()
        self._order_volume_over_sales_person_data_queue = queue.Queue()

    def fix_data(self):
        self.dashboard_data['Sales.SalesOrderHeader'].loc[:, 'TotalDue'] = self.dashboard_data['Sales.SalesOrderHeader']['TotalDue'].str.replace(',', '.')
        self.dashboard_data['Sales.SalesOrderHeader'].loc[:, 'TotalDue'] = pd.to_numeric(self.dashboard_data['Sales.SalesOrderHeader']['TotalDue'], errors='coerce')
        self.dashboard_data['Sales.SalesOrderHeader']['TimeUnitYear'] = self.dashboard_data['Sales.SalesOrderHeader']['OrderDateYear'].astype(str)

        self.dashboard_data['OrderVolumePerCustomer'].loc[:, 'OrderVolume'] = self.dashboard_data['OrderVolumePerCustomer']['OrderVolume'].str.replace(',', '.')
        self.dashboard_data['OrderVolumePerCustomer'].loc[:, 'OrderVolume'] = pd.to_numeric(self.dashboard_data['OrderVolumePerCustomer']['OrderVolume'], errors='coerce')

        self.dashboard_data['OrderVolumePerProduct'].loc[:, 'OrderVolume'] = self.dashboard_data['OrderVolumePerProduct']['OrderVolume'].str.replace(',', '.')
        self.dashboard_data['OrderVolumePerProduct'].loc[:, 'OrderVolume'] = pd.to_numeric(self.dashboard_data['OrderVolumePerProduct']['OrderVolume'], errors='coerce')

        self.dashboard_data['OrderVolumePerSalesPerson'].loc[:, 'OrderVolume'] = self.dashboard_data['OrderVolumePerSalesPerson']['OrderVolume'].str.replace(',', '.')
        self.dashboard_data['OrderVolumePerSalesPerson'].loc[:, 'OrderVolume'] = pd.to_numeric(self.dashboard_data['OrderVolumePerSalesPerson']['OrderVolume'], errors='coerce')

        self.dashboard_data['OrderVolumePerSalesTerritory'].loc[:, 'OrderVolume'] = self.dashboard_data['OrderVolumePerSalesTerritory']['OrderVolume'].str.replace(',', '.')
        self.dashboard_data['OrderVolumePerSalesTerritory'].loc[:, 'OrderVolume'] = pd.to_numeric(self.dashboard_data['OrderVolumePerSalesTerritory']['OrderVolume'], errors='coerce')

        print('Fixed data')

    def create_dashboard(self):
        # Set realistic date limits that Pandas can handle
        min_real_date = '1900-01-01'
        max_real_date = '2200-04-11'  # Upper bound for pandas datetime64

        dashboard = dash.Dash('dashboard_server', self.dashboard_server, external_stylesheets=[dbc.themes.BOOTSTRAP])
        dashboard.layout = dbc.Container(
            [
                html.H1("Order Volume"),
                html.Label("Time unit:"),
                dcc.Dropdown(
                    id="time_unit_selection",
                    options=[
                        {"label": "Calender Week", "value": "cw"},
                        {"label": "Month", "value": "month"},
                        {"label": "Quarter", "value": "quarter"},
                        {"label": "Year", "value": "year"},
                    ],
                    value="month",  # Standardwert
                ),
                html.Label("Time span:"),
                dcc.DatePickerRange(
                    id='date_picker_range',
                    start_date=min_real_date,
                    end_date=max_real_date,
                    display_format='DD.MM.YYYY'  # Format für die Anzeige
                ),
                dcc.Graph(id="order_volume_over_time_graph"),
                html.Hr(),  # Linie zur Trennung
                html.H2("Pie Charts Übersicht"),
                dbc.Row(
                    [
                        dbc.Col(dcc.Graph(id="pie_chart_1"), width=6),  # Pie Chart 1
                        dbc.Col(dcc.Graph(id="pie_chart_2"), width=6),  # Pie Chart 2
                    ]
                ),
                dbc.Row(
                    [
                        dbc.Col(dcc.Graph(id="pie_chart_3"), width=6),  # Pie Chart 3
                        dbc.Col(dcc.Graph(id="pie_chart_4"), width=6),  # Pie Chart 4
                    ]
                ),
            ],
            fluid=True,
        )

        # Callback-Funktion, um das Diagramm dynamisch zu aktualisieren
        @dashboard.callback(
            Output("order_volume_over_time_graph", "figure"),
            [
                Input("time_unit_selection", "value"),
                Input("date_picker_range", "start_date"),
                Input("date_picker_range", "end_date"),
            ],
        )
        def update_order_volume_over_time_graph(time_unit, start_date, end_date):
            update_order_volume_over_time_thread = threading.Thread(target=update_order_volume_over_time_graph_date, args=(time_unit, start_date, end_date))
            update_order_volume_over_time_thread.start()
            print(f"Started thread 'update_order_volume_over_time_graph_date'")

            # Create the bar chart with filtered data
            fig = px.bar(
                self._order_volume_over_time_data_queue.get(),
                x='TimeUnit',
                y='OrderVolume',
                title=f"Order Volume Over {time_unit}",
                labels={"Kategorie": "Kategorie", time_unit: "Wert"},
            )
            return fig

        def update_order_volume_over_time_graph_date(time_unit, start_date, end_date):
            # Ensure valid date range for filtering
            start_date = pd.to_datetime(start_date, errors='coerce')
            end_date = pd.to_datetime(end_date, errors='coerce')

            # Filter the data only for valid date ranges
            if pd.isnull(start_date) or pd.isnull(end_date):
                self._order_volume_over_time_data_queue.put(pd.DataFrame())

            # Filter the data based on selected dates
            dashboard_data = self.dashboard_data['Sales.SalesOrderHeader']
            filtered_data = dashboard_data[
                (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') >= start_date) &
                (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') <= end_date)
                ]

            # Aggregieren der gefilterten Daten
            aggregated_table = aggregate_by_time_unit(filtered_data, time_unit)
            self._order_volume_over_time_data_queue.put(aggregated_table)


        # Dummy Pie Charts Callback
        @dashboard.callback(
            [
                Output("pie_chart_1", "figure"),
                Output("pie_chart_2", "figure"),
                Output("pie_chart_3", "figure"),
                Output("pie_chart_4", "figure"),
            ],
            [
                Input("date_picker_range", "start_date"),
                Input("date_picker_range", "end_date"),
            ],
        )
        def update_pie_charts(start_date, end_date):
            print('Updating pie charts.')
            update_order_volume_over_customer_data_thread = threading.Thread(target=self.update_order_volume_over_customer_data, args=(start_date, end_date))
            update_order_volume_over_customer_data_thread.start()
            update_order_volume_over_product_data_thread = threading.Thread(target=self.update_order_volume_over_product_data, args=(start_date, end_date))
            update_order_volume_over_product_data_thread.start()
            update_order_volume_over_territory_data_thread = threading.Thread(target=self.update_order_volume_over_territory_data, args=(start_date, end_date))
            update_order_volume_over_territory_data_thread.start()
            update_order_volume_over_sales_person_data_thread = threading.Thread(target=self.update_order_volume_over_sales_person_data, args=(start_date, end_date))
            update_order_volume_over_sales_person_data_thread.start()

            # Dummy Data
            dummy_data = {
                'Category': ['A', 'B', 'C', 'D'],
                'Values': [50, 30, 10, 10]
            }
            df = pd.DataFrame(dummy_data)

            # Create order volume per customer chart
            customer_data = self._order_volume_over_customer_data_queue.get()
            total_order_volume = customer_data['OrderVolume'].sum()
            pie_1 = go.Figure(
                data=[go.Pie(labels=customer_data['CustomerName'], values=customer_data['OrderVolume'], hole=0.4)])
            pie_1.update_layout(
                title="Order Volume Over Customer",
                annotations=[dict(
                    text=f'Order volume of top 10 customers: {total_order_volume}',
                    x=0.5, y=-0.1,
                    font_size=15,
                    showarrow=False
                )]
            )

            # Create order volume per product chart
            product_data = self._order_volume_over_product_data_queue.get()
            total_order_volume_product = product_data['OrderVolume'].sum()
            pie_2 = go.Figure(
                data=[go.Pie(labels=product_data['ProductName'], values=product_data['OrderVolume'], hole=0.4)])
            pie_2.update_layout(
                title="Order Volume Over Product",
                annotations=[dict(
                    text=f'Order volume of top 10 product: {total_order_volume_product}',
                    x=0.5, y=-0.1,
                    font_size=15,
                    showarrow=False
                )]
            )

            territory_data = self._order_volume_over_territory_data_queue.get()
            total_order_volume_territory = territory_data['OrderVolume'].sum()
            pie_3 = go.Figure(
                data=[go.Pie(labels=territory_data['TerritoryName'], values=territory_data['OrderVolume'], hole=0.4)])
            pie_3.update_layout(
                title="Order Volume Over Territory",
                annotations=[dict(
                    text=f'Order volume of top 10 territories: {total_order_volume_territory}',
                    x=0.5, y=-0.1,
                    font_size=15,
                    showarrow=False
                )]
            )

            sales_person_data = self._order_volume_over_sales_person_data_queue.get()
            total_order_volume_sales_person = sales_person_data['OrderVolume'].sum()
            pie_4 = go.Figure(
                data=[go.Pie(labels=sales_person_data['SalesPersonName'], values=sales_person_data['OrderVolume'], hole=0.4)])
            pie_4.update_layout(
                title="Order Volume Over Sales Person",
                annotations=[dict(
                    text=f'Order volume of top 10 sales persons: {total_order_volume_sales_person}',
                    x=0.5, y=-0.1,
                    font_size=15,
                    showarrow=False
                )]
            )

            return pie_1, pie_2, pie_3, pie_4

        self.dashboard = dashboard

    def update_order_volume_over_customer_data(self, start_date, end_date):
        # Ensure valid date range for filtering
        print('Updating order value over customer data')
        start_date = pd.to_datetime(start_date, errors='coerce')
        end_date = pd.to_datetime(end_date, errors='coerce')

        # Filter the data only for valid date ranges
        if pd.isnull(start_date) or pd.isnull(end_date):
            self._order_volume_over_customer_data_queue.put(pd.DataFrame())

        # Filter the data based on selected dates
        dashboard_data = self.dashboard_data['OrderVolumePerCustomer']
        filtered_data = dashboard_data[
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') >= start_date) &
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') <= end_date)
            ]

        aggregated_table = aggregate_by_column_with_percentage(filtered_data, 'CustomerID', 'CustomerName', 'OrderVolume')
        print(f'Aggregated OrderVolumePerCustomer over CustomerID. Result: {aggregated_table.shape[0]} rows')
        self._order_volume_over_customer_data_queue.put(aggregated_table)

    def update_order_volume_over_product_data(self, start_date, end_date):
        # Ensure valid date range for filtering
        print('Updating order value over product data')
        start_date = pd.to_datetime(start_date, errors='coerce')
        end_date = pd.to_datetime(end_date, errors='coerce')

        # Filter the data only for valid date ranges
        if pd.isnull(start_date) or pd.isnull(end_date):
            self._order_volume_over_product_data_queue.put(pd.DataFrame())

        # Filter the data based on selected dates
        dashboard_data = self.dashboard_data['OrderVolumePerProduct']
        filtered_data = dashboard_data[
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') >= start_date) &
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') <= end_date)
            ]

        aggregated_table = aggregate_by_column_with_percentage(filtered_data, 'ProductID', 'ProductName', 'OrderVolume')
        print(f'Aggregated OrderVolumePerProduct over ProductID. Result: {aggregated_table.shape[0]} rows')
        self._order_volume_over_product_data_queue.put(aggregated_table)

    def update_order_volume_over_territory_data(self, start_date, end_date):
        # Ensure valid date range for filtering
        print('Updating order value over customer data')
        start_date = pd.to_datetime(start_date, errors='coerce')
        end_date = pd.to_datetime(end_date, errors='coerce')

        # Filter the data only for valid date ranges
        if pd.isnull(start_date) or pd.isnull(end_date):
            self._order_volume_over_territory_data_queue.put(pd.DataFrame())

        # Filter the data based on selected dates
        dashboard_data = self.dashboard_data['OrderVolumePerTerritory']
        print(dashboard_data.head())
        filtered_data = dashboard_data[
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') >= start_date) &
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') <= end_date)
            ]

        aggregated_table = aggregate_by_column_with_percentage(filtered_data, 'TerritoryID', 'TerritoryName', 'OrderVolume')
        print(f'Aggregated OrderVolumePerTerritory over TerritoryID. Result: {aggregated_table.shape[0]} rows')
        self._order_volume_over_territory_data_queue.put(aggregated_table)

    def update_order_volume_over_sales_person_data(self, start_date, end_date):
        # Ensure valid date range for filtering
        print('Updating order value over sales person data')
        start_date = pd.to_datetime(start_date, errors='coerce')
        end_date = pd.to_datetime(end_date, errors='coerce')

        # Filter the data only for valid date ranges
        if pd.isnull(start_date) or pd.isnull(end_date):
            self._order_volume_over_sales_person_data_queue.put(pd.DataFrame())

        # Filter the data based on selected dates
        dashboard_data = self.dashboard_data['OrderVolumePerSalesPerson']
        print(dashboard_data.head())
        filtered_data = dashboard_data[
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') >= start_date) &
            (pd.to_datetime(dashboard_data['OrderDate'], errors='coerce') <= end_date)
            ]

        aggregated_table = aggregate_by_column_with_percentage(filtered_data, 'SalesPersonID', 'SalesPersonName',
                                                               'OrderVolume')
        print(f'Aggregated OrderVolumePerSalesPerson over SalesPersonID. Result: {aggregated_table.shape[0]} rows')
        self._order_volume_over_sales_person_data_queue.put(aggregated_table)
