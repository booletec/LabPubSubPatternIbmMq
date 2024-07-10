namespace IBMMQ.Core.Infra.Abstractions
{
    public class EventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<Subscription>> _eventHandlers;
        public bool IsEmpty => _eventHandlers.Keys.Count != 0;

        public event EventHandler<string>? OnEventRemoved;

        public event EventHandler<string>? OnEventAdded;

        public EventBusSubscriptionManager() => _eventHandlers = [];

        public void AddSubscription<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>
        {
            AddSubscription(typeof(THandler), GetEventKey<TEvent>(), typeof(TEvent));
        }

        public void Clear() => _eventHandlers.Clear();

        public static string GetEventKey<TEvent>() => typeof(TEvent).Name;

        public IEnumerable<Subscription> GetHandlers<TEvent>() where TEvent : Event
        {
            var eventName = GetEventKey<TEvent>();
            return GetHandlers(eventName);
        }

        public bool HasSubscriptions(string eventName) => _eventHandlers.ContainsKey(eventName);

        public void RemoveSubscription<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>
        {
            var eventName = GetEventKey<TEvent>();
            var subsToRemove = GetSubscriptionToRemove(eventName, typeof(THandler));

            if (subsToRemove != null)
                RemoveSubscription(eventName, subsToRemove);

        }

        void AddSubscription(Type handlerType, string eventName, Type eventType)
        {
            if (!HasSubscriptions(eventName))
            {
                _eventHandlers.Add(eventName, []);
                OnEventAdded?.Invoke(this, eventName);
            }

            if (_eventHandlers[eventName].Any(s => s.HandlerType == handlerType))
                throw new ArgumentException($"Handler {handlerType.Name} already register for '{eventName}'", nameof(handlerType));

            _eventHandlers[eventName].Add(Subscription.Create(handlerType, eventType));
        }

        void RemoveSubscription(string eventName, Subscription subsToRemove)
        {
            if (subsToRemove != null)
            {
                _eventHandlers[eventName].Remove(subsToRemove);
                if (_eventHandlers[eventName].Count == 0)
                {
                    _eventHandlers.Remove(eventName);
                    OnEventRemoved?.Invoke(this, eventName);
                }
            }
        }

        Subscription? GetSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptions(eventName))
                return null;

            return _eventHandlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }

        public IEnumerable<Subscription> GetHandlers(string eventName) => _eventHandlers[eventName];
    }
}
