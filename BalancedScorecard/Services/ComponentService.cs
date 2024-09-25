using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public class ComponentService : IComponentService
    {
        public Dictionary<Type,IComponent> Components { get; private set; }

        public ComponentService()
        {
            Components = new Dictionary<Type,IComponent>();
        }
    }
}
