using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IPlotDrawer
    {
        Task DrawOrderVolumeBarPlot(IComponent sender, List<string> xValues, List<decimal> yValues);
        Task DrawOrderVolumePiePlot(IComponent sender, List<string> xValues, List<decimal> yValues, string htmlElement, string title);
        Task DrawHeatmapForOrderVolumeMatrix(IComponent sender);
        //Task DrawFinancesPlots();
    }
}
