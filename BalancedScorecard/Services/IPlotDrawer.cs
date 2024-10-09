using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IPlotDrawer
    {
        Task DrawFinancesPlots(IComponent sender);
    }
}
