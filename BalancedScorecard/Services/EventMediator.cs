using System;
using System.Collections.Generic;

namespace BalancedScorecard.Services
{
    public class EventMediator : IEventMediator
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            _eventHandlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
            }
        }

        public void Publish<T>(T eventToPublish)
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    // Call the handler with the event data
                    ((Action<T>)handler)(eventToPublish);
                }
            }
        }
    }

}
