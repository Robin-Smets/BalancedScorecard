import dash
from dash import dcc, html
import dash_bootstrap_components as dbc
from dash.dependencies import Input, Output
import requests
from flask import Flask, jsonify

# Flask-App erstellen
server = Flask(__name__)

# Eine einfache API-Route erstellen
@server.route('/api/data', methods=['GET'])
def get_data():
    # Beispiel-Daten, die von der API zurückgegeben werden
    data = {'message': 'Hello from Flask API', 'value': 42}
    return jsonify(data)

# Dash-App erstellen, die auf demselben Flask-Server läuft
app = dash.Dash(__name__, server=server, external_stylesheets=[dbc.themes.BOOTSTRAP])

# Layout der Dash-App
app.layout = dbc.Container(
    [
        html.H1("Dash App mit Flask API Integration"),
        html.Button("Daten von API abrufen", id="get-data-btn", n_clicks=0),
        html.Div(id="output")
    ],
    fluid=True,
)

# Callback für die Schaltfläche, die Daten von der API abruft
@app.callback(
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

# Dash-Anwendung starten
if __name__ == "__main__":
    app.run_server(debug=True)
