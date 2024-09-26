using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;
using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public class ComponentService : IComponentService
    {
        public IComponent RoutedPage { get; set; }

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

        private readonly IServiceProvider _services;

        public ComponentService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task LoadDataButtonClick()
        {
            await _services.GetRequiredService<IDataStoreService>().LoadData();
            await _services.GetRequiredService<IPlotDrawer>().DrawFinancesPlots();
        }


    }
}
