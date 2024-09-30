// ComponentService.cs

using BalancedScorecard.Components.Pages;
using BalancedScorecard.Enums;
using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    /// <summary>
    /// Implementation of IComponentService.
    /// </summary>
    public class ComponentService : IComponentService
    {
        public IComponent RoutedPage { get; set; }

        /// <summary>
        /// The DI container.
        /// </summary>
        private readonly IServiceProvider _services;

        /// <summary>
        /// Reference to the datastore service via DI container.
        /// </summary>
        private IDataStoreService _dataStoreService => _services.GetRequiredService<IDataStoreService>();
        /// <summary>
        /// Reference to the plot drawer via DI container.
        /// </summary>
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
