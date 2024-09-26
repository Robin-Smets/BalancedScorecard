using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IPlotDrawer
    {

        Task DrawHeatmapForOrderVolumeMatrix(IComponent sender);
        Task DrawFinancesPlots();
    }
}
