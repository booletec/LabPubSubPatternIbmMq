using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Ibmmq.Core.Conectors
{
    internal class Subscription
    {

        public Type HandlerType { get; }
        public Type EventType { get; }

        public Subscription(Type handlerType, Type eventType)
        {
            HandlerType = handlerType;
            EventType = eventType;
        }

        public async Task Handle(string message, IServiceScope scope)
        {
            var eventData = JsonSerializer.Deserialize(message, EventType);
            var concreteType = typeof(IEventHandler<>).MakeGenericType(EventType);

            if (concreteType is null) throw new Exception("Handler for this event not implemented yet");
            var handler = scope.ServiceProvider.GetRequiredService(concreteType);

            await (Task)concreteType.GetMethod("Handle").Invoke(handler, [eventData]);

        }

        public static Subscription Create(Type handlerType, Type eventType) => new(handlerType, eventType);
    }
}
