namespace Ibmmq.Core.Conectors
{
    public interface IEventHandler<in TEvent> : IEventHandler
     where TEvent : Event
    {
        Task Handle(TEvent @event); 
    }

    public interface IEventHandler
    {
    }
}
