using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Events
{
    public class VisualStateChangedEvent
    {
        public IComponent Sender { get; private set; }

        public VisualStateChangedEvent(IComponent sender)
        {
            Sender = sender;
        }
    }
}
