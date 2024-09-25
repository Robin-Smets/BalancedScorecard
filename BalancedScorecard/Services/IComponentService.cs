using Microsoft.AspNetCore.Components;

namespace BalancedScorecard.Services
{
    public interface IComponentService
    {
        Dictionary<Type, IComponent> Components { get; }
    }
}
