using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public class AppState : IAppState
    {
        public DateTime FromDateFilter { get; set; }
        public DateTime UntilDateFilter { get; set; }
        public Tuple<string, string> RevenueBarPlotWhereFilter { get; set; }
        public IComponent? RoutedPage { get; set; }
        public string RevenueBarPlotSelectedTimeUnit {  get; set; }

        public AppState()
        {
            FromDateFilter = DateTime.Now;
            UntilDateFilter = DateTime.Now;
            RevenueBarPlotWhereFilter = Tuple.Create("", "");
            RevenueBarPlotSelectedTimeUnit = "Month";
        }
    }
}
