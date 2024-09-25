using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Data;
using BalancedScorecard.Events;
using System.Globalization;
using System;
using BalancedScorecard.Components.Pages;

namespace BalancedScorecard.Services
{
    public class PlotDrawer : IPlotDrawer
    {
        private IDataStoreService _dataStoreService;
        private IEventMediator _eventMediator;
        private IJSRuntime _jSRuntime;
        private IComponentService _componentService;
        private ITransformer _transformer;
        

        public PlotDrawer(IDataStoreService dataStoreService, IJSRuntime jSRuntime, IEventMediator eventMediator, IComponentService componentService, ITransformer transformer)
        {
            _dataStoreService = dataStoreService;
            _jSRuntime = jSRuntime;
            _eventMediator = eventMediator;
            _componentService = componentService;
            _transformer = transformer;
        }

        public async Task DrawOrderVolumeBarPlot(IComponent sender)
        {
            var xValues = new List<string>();
            var yValues = new List<decimal>();
            var financesComponent = _componentService.Components[typeof(Finances)] as Finances;
            var groupByColumn = "";

            switch (financesComponent.SelectedTimeUnit)
            {
                case "CW":
                    groupByColumn = "TimeUnitCalenderWeek";
                    break;

                case "Month":
                    groupByColumn = "TimeUnitMonth";
                    break;

                case "Quarter":
                    groupByColumn = "TimeUnitQuarter";
                    break;

                case "Year":
                    groupByColumn = "TimeUnitYear";
                    break;
            }
            

            if (_dataStoreService.DataTables["OrderVolume"] is not null)
            {
                var orderVolumeData = _dataStoreService.DataTables["OrderVolume"]
                .AsEnumerable()
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()).Value >= _dataStoreService.FromDateFilter)
                .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()).Value <= _dataStoreService.UntilDateFilter)
                .GroupBy(row => row[groupByColumn].ToString());

                foreach (var group in orderVolumeData)
                {
                    var totalOrderVolume = group.Sum(row => Convert.ToDecimal(row["OrderVolume"]));

                    xValues.Add(group.Key);
                    yValues.Add(totalOrderVolume);
                }
            }

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

        public async Task DrawOrderVolumePiePlots(IComponent sender)
        {
            await DrawOrderVolumePiePlot(sender, "CustomerID", "order-volume-customer-pie");
            await DrawOrderVolumePiePlot(sender, "ProductID", "order-volume-product-pie");
            await DrawOrderVolumePiePlot(sender, "SalesPersonID", "order-volume-sales-person-pie");
            await DrawOrderVolumePiePlot(sender, "TerritoryID", "order-volume-territory-pie");
        }

        public async Task DrawOrderVolumePiePlot(IComponent sender, string groupByColumn, string plotId)
        {
            var xValues = new List<string>();
            var yValues = new List<decimal>();

            if (_dataStoreService.DataTables["OrderVolume"] is not null)
            {
                var orderVolumeData = _dataStoreService.DataTables["OrderVolume"]
                    .AsEnumerable()
                    .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()).Value >= _dataStoreService.FromDateFilter)
                    .Where(x => CreateDateTimeFromString(x["OrderDate"].ToString()).Value <= _dataStoreService.UntilDateFilter)
                    .GroupBy(row => row[groupByColumn].ToString())  // Gruppierung nach Spalte
                    .Select(group => new
                    {
                        GroupKey = group.Key,
                        GroupData = group,
                        TotalOrderVolumePercentage = group.Sum(x => Convert.ToDecimal(x["OrderVolumePercentage"])) // Berechnung der Summe für jede Gruppe
                    })
                    .OrderByDescending(group => group.TotalOrderVolumePercentage)  // Sortierung in absteigender Reihenfolge
                    .Take(10);

                foreach (var group in orderVolumeData)
                {
                    var totalOrderVolume = group.GroupData.Sum(row => Convert.ToDecimal(row["OrderVolume"]));

                    var groupKeyNameColumn = groupByColumn.Replace("ID", "Name");
                    var firstMatchingRow = group.GroupData.First(x => x[groupByColumn].ToString() == group.GroupKey);
                    var groupKeyName = firstMatchingRow[groupKeyNameColumn].ToString();

                    xValues.Add(groupKeyName);
                    yValues.Add(totalOrderVolume);
                }
            }

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
                title = $"Order Volume Over {groupByColumn.Replace("ID", "")}"
            };

            await _jSRuntime.InvokeVoidAsync("createPlot", plotId, data, layout);
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

        private DateTime? CreateDateTimeFromString(string dateString, string format = "dd.MM.yyyy HH:mm:ss")
        {
            try
            {
                var dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (FormatException)
            {
                Console.WriteLine("Das Format des Datums ist ungültig.");
                return null;
            }
        }
    }
}
