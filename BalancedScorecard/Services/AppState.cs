using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public class AppState : IAppState
    {
        public IComponent RoutedPage { get; set; }
        public DateTime? FromDateFilter { get; set; }
        public DateTime? UntilDateFilter { get; set; }

        public AppState()
        {
            FromDateFilter = DateTime.Now;
            UntilDateFilter = DateTime.Now;
        }
    }
}
