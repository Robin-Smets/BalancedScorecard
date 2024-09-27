using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Data;
using BalancedScorecard.Events;
using System.Globalization;
using System;
using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;
using System.Data.Odbc;
using System.Data.Common;
using System.IO.Pipelines;

namespace BalancedScorecard.Services
{
    public class PlotDrawer : IPlotDrawer
    {
        private IDataStoreService _dataStoreService;
        private IEventMediator _eventMediator;
        private IJSRuntime _jSRuntime;
        private IComponentService _componentService;
        private ITransformer _transformer;

        private KeyValuePair<string,string> _orderVolumeQuery => _dataStoreService.GetSqlFilesContent("./Sql/Tables/").First();


        public PlotDrawer(IDataStoreService dataStoreService, IJSRuntime jSRuntime, IEventMediator eventMediator, IComponentService componentService, ITransformer transformer)
        {
            _dataStoreService = dataStoreService;
            _jSRuntime = jSRuntime;
            _eventMediator = eventMediator;
            _componentService = componentService;
            _transformer = transformer;
        }

        public async Task DrawFinancesPlots()
        {
            // Starte alle Tasks parallel
            var groupedBarPlotTask = CreateGroupedDataSource(GetTimeUnitColumn(), _orderVolumeQuery);
            var customerPiePlotTask = CreateGroupedDataSource("CustomerID", _orderVolumeQuery, true, 10, true);
            var productPiePlotTask = CreateGroupedDataSource("ProductID", _orderVolumeQuery, true, 10, true);
            var salesPersonPiePlotTask = CreateGroupedDataSource("SalesPersonID", _orderVolumeQuery, true, 10, true);
            var territoryPiePlotTask = CreateGroupedDataSource("TerritoryID", _orderVolumeQuery, true, 10, true);

            // Warte auf alle Tasks gleichzeitig
            await Task.WhenAll(groupedBarPlotTask, customerPiePlotTask, productPiePlotTask, salesPersonPiePlotTask, territoryPiePlotTask);

            // Extrahiere die Ergebnisse aus den Tasks
            var groupedBarPlotDataSource = await groupedBarPlotTask;
            var customerPiePlotDataSource = await customerPiePlotTask;
            var productPiePlotDataSource = await productPiePlotTask;
            var salesPersonPiePlotDataSource = await salesPersonPiePlotTask;
            var territoryPiePlotDataSource = await territoryPiePlotTask;

            // Zeichne die Diagramme
            DrawOrderVolumeBarPlot(_componentService.RoutedPage, groupedBarPlotDataSource.Item1, groupedBarPlotDataSource.Item2);
            DrawOrderVolumePiePlot(_componentService.RoutedPage, "CustomerID", "order-volume-customer-pie", customerPiePlotDataSource.Item1, customerPiePlotDataSource.Item2);
            DrawOrderVolumePiePlot(_componentService.RoutedPage, "ProductID", "order-volume-product-pie", productPiePlotDataSource.Item1, productPiePlotDataSource.Item2);
            DrawOrderVolumePiePlot(_componentService.RoutedPage, "SalesPersonID", "order-volume-sales-person-pie", salesPersonPiePlotDataSource.Item1, salesPersonPiePlotDataSource.Item2);
            DrawOrderVolumePiePlot(_componentService.RoutedPage, "TerritoryID", "order-volume-territory-pie", territoryPiePlotDataSource.Item1, territoryPiePlotDataSource.Item2);
        }


        private async Task<(List<string>, List<decimal>)> CreateGroupedDataSource(string groupByColumn, KeyValuePair<string, string> sqlScript, bool ordered=false, int take=0, bool isIdColumn=false)
        {
            var plotXValues = new List<string>();
            var plotYValues = new List<decimal>();
            var plotGroupByColumn = GetTimeUnitColumn();
            var plotGroups = new Dictionary<string, decimal>();
            var groupNames = new Dictionary<string, string>();

            using (OdbcConnection connection = new OdbcConnection(_dataStoreService.ConnectionString))
            {
                await connection.OpenAsync();

                using (OdbcCommand command = new OdbcCommand(sqlScript.Value, connection))
                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        var orderDate = CreateDateTimeFromString(reader["OrderDate"].ToString());
                        if (orderDate.HasValue && orderDate.Value >= _dataStoreService.FromDateFilter && orderDate.Value <= _dataStoreService.UntilDateFilter)
                        {
                            var groupKey = reader[groupByColumn].ToString();
                            var aggregatableValue = Convert.ToDecimal(reader["OrderVolumePercentage"]);

                            if (plotGroups.ContainsKey(groupKey))
                            {
                                plotGroups[groupKey] += aggregatableValue;
                                if (isIdColumn)
                                {
                                    groupNames[groupKey] = reader[groupByColumn.Replace("ID", "Name")].ToString();
                                }
                            }
                            else
                            {
                                plotGroups[groupKey] = aggregatableValue;
                                if (isIdColumn)
                                {
                                    groupNames[groupKey] = reader[groupByColumn.Replace("ID", "Name")].ToString();
                                }
                            }
                        }
                    }
                }         
            }

            IOrderedEnumerable<KeyValuePair<string, decimal>> plotGroupsEnumerable;
            if (ordered)
            {
                plotGroupsEnumerable = plotGroups.OrderByDescending(group => group.Value);
            }
            else
            {
                plotGroupsEnumerable = plotGroups as IOrderedEnumerable<KeyValuePair<string, decimal>>;
            }

            if (take > 0)
            {
                if (plotGroupsEnumerable != null)
                {
                    plotGroups = plotGroupsEnumerable.Take(take).ToDictionary();
                }
            }

            if (isIdColumn)
            {
                foreach (var group in plotGroups)
                {
                    plotXValues.Add(groupNames[group.Key]);
                    plotYValues.Add(group.Value);
                }
            }
            else
            {
                foreach (var group in plotGroups)
                {
                    plotXValues.Add(group.Key);
                    plotYValues.Add(group.Value);
                }
            }
            return (plotXValues, plotYValues);
        }

        private string GetTimeUnitColumn()
        {
            var financesComponent = _componentService.RoutedPage as Finances;
            var timeUnitColumn = "";

            switch (financesComponent.SelectedTimeUnit)
            {
                case "CW":
                    timeUnitColumn = "TimeUnitCalenderWeek";
                    break;

                case "Month":
                    timeUnitColumn = "TimeUnitMonth";
                    break;

                case "Quarter":
                    timeUnitColumn = "TimeUnitQuarter";
                    break;

                case "Year":
                    timeUnitColumn = "TimeUnitYear";
                    break;
            }

            return timeUnitColumn;
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

        public async Task DrawOrderVolumePiePlot(IComponent sender, string groupByColumn, string plotId, List<string> xValues, List<decimal> yValues)
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
