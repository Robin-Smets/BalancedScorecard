import dash
import pandas as pd
import plotly.express as px
from dash import html, dcc
import dash_bootstrap_components as dbc
from dash.dependencies import Input, Output
from data import aggregate_by_time_unit


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

    def fix_data(self):
        self.dashboard_data.loc[:, 'TotalDue'] = self.dashboard_data['TotalDue'].str.replace(',', '.')
        self.dashboard_data.loc[:, 'TotalDue'] = pd.to_numeric(self.dashboard_data['TotalDue'], errors='coerce')
        self.dashboard_data['TimeUnitYear'] = self.dashboard_data['OrderDateYear'].astype(str)

    def create_dashboard(self):
        # Set realistic date limits that Pandas can handle
        min_real_date = '1900-01-01'
        max_real_date = '2200-04-11'  # Upper bound for pandas datetime64

        dashboard = dash.Dash('dashboard_server', self.dashboard_server, external_stylesheets=[dbc.themes.BOOTSTRAP])
        dashboard.layout = dbc.Container(
            [
                html.H1("Interaktives Balkendiagramm mit Dash"),
                html.Label("Zeiteinheit:"),
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
                html.Label("Zeitraum auswählen:"),
                dcc.DatePickerRange(
                    id='date_picker_range',
                    start_date=min_real_date,
                    end_date=max_real_date,
                    display_format='DD.MM.YYYY'  # Format für die Anzeige
                ),
                dcc.Graph(id="balkendiagramm"),
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
            Output("balkendiagramm", "figure"),
            [
                Input("time_unit_selection", "value"),
                Input("date_picker_range", "start_date"),
                Input("date_picker_range", "end_date"),
            ],
        )
        def update_balkendiagramm(time_unit, start_date, end_date):
            # Ensure valid date range for filtering
            start_date = pd.to_datetime(start_date, errors='coerce')
            end_date = pd.to_datetime(end_date, errors='coerce')

            # Filter the data only for valid date ranges
            if pd.isnull(start_date) or pd.isnull(end_date):
                return px.bar()  # Return an empty figure if dates are invalid

            # Filter the data based on selected dates
            filtered_data = self.dashboard_data[
                (pd.to_datetime(self.dashboard_data['OrderDate'], errors='coerce') >= start_date) &
                (pd.to_datetime(self.dashboard_data['OrderDate'], errors='coerce') <= end_date)
            ]

            # Aggregieren der gefilterten Daten
            aggregated_table = aggregate_by_time_unit(filtered_data, time_unit)

            # Create the bar chart with filtered data
            fig = px.bar(
                aggregated_table,
                x='TimeUnit',
                y='OrderVolume',
                title=f"Balkendiagramm für {time_unit} von {start_date.date()} bis {end_date.date()}",
                labels={"Kategorie": "Kategorie", time_unit: "Wert"},
            )
            return fig

        # Dummy Pie Charts Callback
        @dashboard.callback(
            [
                Output("pie_chart_1", "figure"),
                Output("pie_chart_2", "figure"),
                Output("pie_chart_3", "figure"),
                Output("pie_chart_4", "figure"),
            ],
            [
                Input("time_unit_selection", "value"),
            ],
        )
        def update_pie_charts(time_unit):
            # Dummy Data
            dummy_data = {
                'Category': ['A', 'B', 'C', 'D'],
                'Values': [50, 30, 10, 10]
            }
            df = pd.DataFrame(dummy_data)

            # Create four pie charts with dummy data
            pie_1 = px.pie(df, names='Category', values='Values', title="Pie Chart 1")
            pie_2 = px.pie(df, names='Category', values='Values', title="Pie Chart 2")
            pie_3 = px.pie(df, names='Category', values='Values', title="Pie Chart 3")
            pie_4 = px.pie(df, names='Category', values='Values', title="Pie Chart 4")

            return pie_1, pie_2, pie_3, pie_4

        self.dashboard = dashboard
