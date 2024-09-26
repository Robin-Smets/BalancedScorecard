using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IPlotDrawer
    {
        Task DrawOrderVolumeBarPlot(IComponent sender);
        Task DrawOrderVolumePiePlots(IComponent sender);
        Task DrawHeatmapForOrderVolumeMatrix(IComponent sender);
        Task DrawFinancesPlots();
    }
}
