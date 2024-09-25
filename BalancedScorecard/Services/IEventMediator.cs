namespace BalancedScorecard.Services
{
    public interface IEventMediator
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T eventToPublish);
    }
}
