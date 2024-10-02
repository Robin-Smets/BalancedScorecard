using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Data;
using BalancedScorecard.Events;
using System.Globalization;
using System;
using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;

namespace BalancedScorecard.Services
{
    public class PlotDrawer : IPlotDrawer
    {
        private IJSRuntime _jSRuntime;

        private readonly IServiceProvider _services;
        private IDataStoreService _dataStoreService => _services.GetRequiredService<IDataStoreService>();
        private IEventMediator _eventMediator => _services.GetRequiredService<IEventMediator>();
        private ITransformer _transformer => _services.GetRequiredService<ITransformer>();
        
        public PlotDrawer(IServiceProvider services, IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
            _services = services;
        }

        public async Task DrawOrderVolumeBarPlot(IComponent sender, List<string> xValues, List<decimal> yValues)
        {
            var data = new[]
            {
                new
                {
                    x = xValues.ToArray(),
                    y = yValues.ToArray(),
                    type = "bar"
                }
            };

            var layout = new
            {
                title = "Order Volume Over Time"
            };

            await _jSRuntime.InvokeVoidAsync("createPlot", "order_volume_bar_plot", data, layout);
            _eventMediator.Publish<VisualStateChangedEvent>(new VisualStateChangedEvent(sender));
        }

        public async Task DrawOrderVolumePiePlot(IComponent sender, List<string> xValues, List<decimal> yValues, string htmlElement, string title)
        {
            var data = new[]
            {
                new
                {
                    labels = xValues.ToArray(),
                    values = yValues.ToArray(),
                    type = "pie"
                }
            };

            var layout = new
            {
                title = $"Order Volume Over {title}"
            };

            await _jSRuntime.InvokeVoidAsync("createPlot", htmlElement, data, layout);
            _eventMediator.Publish<VisualStateChangedEvent>(new VisualStateChangedEvent(sender));
        }

        public async Task DrawHeatmapForOrderVolumeMatrix(IComponent sender)
        {
            var topTenCustomers = _transformer.GetTopTenIDs("CustomerID");
            var topTenProducts = _transformer.GetTopTenIDs("ProductID");
            var topTenSalesPersons = _transformer.GetTopTenIDs("SalesPersonID");
            var topTenTerritorys = _transformer.GetTopTenIDs("TerritoryID");
            var filteredData = _transformer.FilterDataTableByTopTenIDs(_dataStoreService.DataTables["OrderVolume"], topTenSalesPersons, topTenCustomers, topTenProducts, topTenTerritorys);
            var orderVolumeMatrix = _transformer.CreateAverageOrderVolumeMatrix(filteredData);
            // Schritt 1: X-Achsen-Werte und Y-Achsen-Werte (Spalten und Zeilen)
            var xValues = new List<string>();
            var yValues = new List<string>();

            // Schritt 2: Z-Werte (die eigentlichen Heatmap-Daten) vorbereiten
            var zValues = new List<List<decimal>>();

            // Füge die Spaltenüberschriften (außer der YAxis-Spalte) zur X-Achse hinzu
            foreach (DataColumn column in orderVolumeMatrix.Columns)
            {
                if (column.ColumnName != "YAxis") // YAxis ist für die Y-Achse
                {
                    xValues.Add(column.ColumnName);
                }
            }

            // Iteriere durch die Reihen der Tabelle und fülle die Y-Achse und die Z-Werte
            foreach (DataRow row in orderVolumeMatrix.Rows)
            {
                // Y-Achsen-Wert (entspricht der YAxis-Spalte)
                yValues.Add(row["YAxis"].ToString());

                // Z-Werte für diese Zeile (Werte aus den übrigen Spalten)
                var zRow = new List<decimal>();
                foreach (var x in xValues)
                {
                    zRow.Add(row.Field<decimal>(x));
                }
                zValues.Add(zRow);
            }

            // Schritt 3: Heatmap-Daten für Plotly.js vorbereiten
            var data = new[]
            {
                new
                {
                    z = zValues.Select(row => row.ToArray()).ToArray(),
                    x = xValues.ToArray(),
                    y = yValues.ToArray(),
                    type = "heatmap"
                }
            };

            // Schritt 4: Layout für die Heatmap erstellen
            var layout = new
            {
                title = "Average Order Volume Over Feature Combination",
                xaxis = new { title = "", automargin = true },
                yaxis = new { title = "", automargin = true },
                margin = new { l=80, r=30, t=40, b=80 }
            };

            // Schritt 5: Plotly.js über JSRuntime aufrufen, um die Heatmap zu rendern
            await _jSRuntime.InvokeVoidAsync("createPlot", "order_volume_heat_map", data, layout);
            _eventMediator.Publish<VisualStateChangedEvent>(new VisualStateChangedEvent(sender));
        }
    }
}
