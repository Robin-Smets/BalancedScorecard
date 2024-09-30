using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;
using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public class ComponentService : IComponentService
    {
        public IComponent RoutedPage { get; set; }

        private readonly IServiceProvider _services;
        private IDataStoreService _dataStoreService => _services.GetRequiredService<IDataStoreService>();
        private IPlotDrawer _plotDrawer => _services.GetRequiredService<IPlotDrawer>();

        public ComponentService(IServiceProvider services)
        {
            _services = services;
        }

        public PageComponent GetRoutedPageEnum()
        {
            if (RoutedPage is Overview)
            {
                return PageComponent.Overview;
            }
            else if (RoutedPage is Finances)
            {
                return PageComponent.Finances;
            }
            else if (RoutedPage is Administration)
            {
                return PageComponent.Administration;
            }

            throw new NotImplementedException($"There is no 'PageComponent' enum for {RoutedPage.GetType().Name}");
        }

        public async Task LoadDataButtonClick()
        {
            await _dataStoreService.LoadData();
            await _plotDrawer.DrawFinancesPlots();
        }
    }
}
