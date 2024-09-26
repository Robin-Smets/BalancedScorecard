using BalancedScorecard.Enums;
using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IComponentService
    {
        IComponent RoutedPage { get; set; }
        PageComponent GetRoutedPageEnum();
        Task LoadDataButtonClick();
    }
}
