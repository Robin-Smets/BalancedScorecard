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
        

        public PlotDrawer(IDataStoreService dataStoreService, IJSRuntime jSRuntime, IEventMediator eventMediator, IComponentService componentService)
        {
            _dataStoreService = dataStoreService;
            _jSRuntime = jSRuntime;
            _eventMediator = eventMediator;
            _componentService = componentService;
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
