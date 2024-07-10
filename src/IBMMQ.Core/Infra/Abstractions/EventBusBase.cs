using Microsoft.Extensions.DependencyInjection;

namespace IBMMQ.Core.Infra.Abstractions
{
    public abstract class EventBus : IEventBus
    {
        protected readonly EventBusSubscriptionManager _evSubscriptionManager;
        protected readonly IServiceProvider? _provider;

        protected EventBus(IServiceProvider? provider = null)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();
            _provider = provider;
        }

        public abstract Task Listen<TEvent>() where TEvent : Event, new();

        public abstract void Publish(Event @event);

        public abstract Task<string> PublishAsync(Event @event);

        protected async Task<bool> ProcessEvent(string eventName, string message, IServiceProvider? _provider)
        {
            var processed = false;

            if (_evSubscriptionManager.HasSubscriptions(eventName))
            {
                using (var scope = _provider?.CreateAsyncScope())
                {
                    var subscriptions = _evSubscriptionManager.GetHandlers(eventName);
                    foreach (var subscription in subscriptions)
                        await subscription.Handle(message, scope);
                }
                processed = true;
            }
            return processed;
        }

        public void Subscribe<TEvent, THandler>()
           where TEvent : Event
           where THandler : IEventHandler<TEvent> => _evSubscriptionManager.AddSubscription<TEvent, THandler>();

        public void Unsubscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent> => _evSubscriptionManager.RemoveSubscription<TEvent, THandler>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _evSubscriptionManager.Clear();

    }
}
