// IComponentService.cs

using BalancedScorecard.Enums;
using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    /// <summary>
    /// Service that contains component logic which has to be globally accesible.
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// The page component that is currently routed.
        /// </summary>
        IComponent RoutedPage { get; set; }

        /// <summary>
        /// Gets an enum to represent the currently routed page component.
        /// </summary>
        /// <returns>Returns currently routed page component type as enum.</returns>
        /// <exception cref="NotImplementedException">Throws if there is no corresponding value in the enum for the page component.</exception>
        PageComponent GetRoutedPageEnum();

        /// <summary>
        /// Button click handler for LoadDataButton in Layout. 
        /// </summary>
        /// <returns></returns>
        Task LoadDataButtonClick();
    }
}
