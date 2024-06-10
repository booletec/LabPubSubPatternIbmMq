namespace Ibmmq.Core.Conectors
{
    internal interface IEventBus : IDisposable
    {
        void Publish(Event @event);

        void Subscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent>;
    }
}
