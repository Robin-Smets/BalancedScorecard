// Events.cs

using BalancedScorecard.Enums;
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

    /// <summary>
    /// A button was clicked that needs to be handled via the event mediator.
    /// </summary>
    public class ButtonClickEvent
    {
        /// <summary>
        /// The button that was clicked.
        /// </summary>
        public RaisingButton Button { get; private set; }

        public ButtonClickEvent(RaisingButton button)
        {
            Button = button;
        }
    }
}
