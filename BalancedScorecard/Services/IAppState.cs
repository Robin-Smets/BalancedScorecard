﻿using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IAppState
    {
        IComponent? RoutedPage { get; set; }
        DateTime FromDateFilter { get; set; }
        DateTime UntilDateFilter { get; set; }
        Tuple<string, string> RevenueBarPlotWhereFilter { get; set; }
        public string RevenueBarPlotSelectedTimeUnit {  get; set; }

    }
}
