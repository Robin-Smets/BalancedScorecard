// Events.cs

using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Events
{
    /// <summary>
    /// The state of the view (or their properties) has changed in a way that rerendering is required.
    /// </summary>
    public class VisualStateChangedEvent
    {
        /// <summary>
        /// The component that was changed.
        /// </summary>
        public IComponent Sender { get; private set; }

        public VisualStateChangedEvent(IComponent sender)
        {
            Sender = sender;
        }
    }
}
