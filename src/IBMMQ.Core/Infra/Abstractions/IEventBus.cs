namespace IBMMQ.Core.Infra.Abstractions
{
    internal interface IEventBus : IDisposable
    {
        void Publish(Event @event);
        Task<string> PublishAsync(Event @event);
        Task Listen<TEvent>() where TEvent : Event, new();

        void Subscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>;
    }
}
