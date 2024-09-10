# dashboard.py

import dash
from dash import html
import dash_bootstrap_components as dbc
from dash.dependencies import Input, Output
import requests

def create_dashboard(dashboard_server):

    dashboard = dash.Dash('dashboard_server', dashboard_server, external_stylesheets=[dbc.themes.BOOTSTRAP])
    dashboard.layout = dbc.Container(
        [
            html.H1("Dash App mit Flask API Integration"),
            html.Button("Daten von API abrufen", id="get-data-btn", n_clicks=0),
            html.Div(id="output")
        ],
        fluid=True,
    )

    # Callback für die Schaltfläche, die Daten von der API abruft
    @dashboard.callback(
        Output("output", "children"),
        Input("get-data-btn", "n_clicks"),
    )
    def update_output(n_clicks):
        if n_clicks > 0:
            # Anfrage an die Flask-API
            response = requests.get("http://127.0.0.1:8050/api/data")
            if response.status_code == 200:
                data = response.json()
                return f"Nachricht von API: {data['message']}, Wert: {data['value']}"
        return "Klicke auf den Button, um Daten von der API abzurufen."

    return dashboard
