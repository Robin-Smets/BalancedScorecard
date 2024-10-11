using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Data;
using BalancedScorecard.Events;
using System.Globalization;
using System;
using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;
using static TorchSharp.torch.utils;

namespace BalancedScorecard.Services
{
    public class PlotDrawer : IPlotDrawer
    {
        private IJSRuntime _jSRuntime;

        private readonly IServiceProvider _services;
        private IDataStoreService _dataStoreService => _services.GetRequiredService<IDataStoreService>();
        private IEventMediator _eventMediator => _services.GetRequiredService<IEventMediator>();
        private ITransformer _transformer => _services.GetRequiredService<ITransformer>();
        private IAppState _appState => _services.GetRequiredService<IAppState>();

        public PlotDrawer(IServiceProvider services, IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
            _services = services;
        }

        public async Task DrawFinancesPlots(IComponent sender)
        {
            try
            {
                var fromDateFilter = _appState.FromDateFilter.ToStartOfDay();
                var untilDateFilter = _appState.UntilDateFilter.ToEndOfDay();
                var timeUnit = _appState.RevenueBarPlotSelectedTimeUnit;
                var whereFilter = _appState.RevenueBarPlotWhereFilter;

                var plotTasks = new List<Task>();

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreatePlotDataSource(timeUnit, fromDateFilter, untilDateFilter, whereFilter: whereFilter);
                        var title = $"Revenue by {timeUnit.ToLower()}";
                        if (whereFilter.Item1 != "" && whereFilter.Item2 != "")
                        {
                            title += $" ({whereFilter.Item1} = {whereFilter.Item2})";
                        }
                        await DrawPlot(sender, dataSource.Item1, dataSource.Item2, "revenue-bar-plot", "bar", title);
                    })
                );

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreatePlotDataSource("Customer", fromDateFilter, untilDateFilter, 10, true);
                        await DrawPlot(sender, dataSource.Item1, dataSource.Item2, "revenue-customer-pie", "pie", "Top ten total revenue by customer");
                    })
                );

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreatePlotDataSource("Product", fromDateFilter, untilDateFilter, 10, true);
                        await DrawPlot(sender, dataSource.Item1, dataSource.Item2, "revenue-product-pie", "pie", "Top ten total revenue by product");
                    })
                );

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreatePlotDataSource("SalesPerson", fromDateFilter, untilDateFilter, 10, true);
                        await DrawPlot(sender, dataSource.Item1, dataSource.Item2, "revenue-sales-person-pie", "pie", "Top ten total revenue by sales person");
                    })
                );

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreatePlotDataSource("Territory", fromDateFilter, untilDateFilter, 10, true);
                        await DrawPlot(sender, dataSource.Item1, dataSource.Item2, "revenue-territory-pie", "pie", "Top ten total revenue by territory");
                    })
                );

                plotTasks.Add(
                    Task.Run(async () =>
                    {
                        var dataSource = await _dataStoreService.CreateHeatMapDataSource(fromDateFilter, untilDateFilter);
                        await DrawPlot(sender, dataSource.Item1, new List<decimal>(), "revenue-heatmap", "heatmap", "Revenue by feature combination", dataSource.Item3);
                    })
                );

                await Task.WhenAll(plotTasks);

                Console.WriteLine("Finances plots were created succesfully.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

        }

        public async Task DrawPlot(IComponent sender, List<string> xValues, List<decimal> yValues, string htmlElement, string type, string title, decimal[,] z = null)
        {
            object[] data = new object[]
            {
        type switch
        {
            "pie" => new { labels = xValues.ToArray(), values = yValues.ToArray(), type },
            "bar" => new { x = xValues.ToArray(), y = yValues.ToArray(), type },
            "heatmap" => new { x = xValues.ToArray(), y = xValues.ToArray(), z = z != null ? ConvertToJaggedArray(z) : null, type },
            _ => throw new ArgumentException($"Unsupported plot type: {type}")
        }
            };

            object layout = type switch
            {
                "pie" => new { title },
                "bar" => new { title },
                "heatmap" => new
                {
                    title,
                    xaxis = new { title = "", automargin = true },
                    yaxis = new { title = "", automargin = true },
                    margin = new { l = 80, r = 30, t = 40, b = 80 }
                },
                _ => throw new ArgumentException($"Unsupported plot type: {type}")
            };

            await _jSRuntime.InvokeVoidAsync("createPlot", htmlElement, data, layout);
            _eventMediator.Publish<VisualStateChangedEvent>(new VisualStateChangedEvent(sender));
        }


        private List<List<decimal>> ConvertToJaggedArray(decimal[,] input)
        {
            var result = new List<List<decimal>>();
            for (int i = 0; i < input.GetLength(0); i++)
            {
                var row = new List<decimal>();
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    row.Add(input[i, j]);
                }
                result.Add(row);
            }
            return result;
        }
    }
}
