using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace IBMMQ.Core.Infra.Abstractions
{
    public class Subscription(Type handlerType, Type eventType)
    {
        public Type HandlerType { get; } = handlerType;
        public Type EventType { get; } = eventType;

        public async Task Handle(string message, IServiceScope? scope)
        {
            var eventData = JsonSerializer.Deserialize(message, EventType);
            var concreteType = typeof(IEventHandler<>).MakeGenericType(EventType) ?? throw new Exception("Handler for this event not implemented yet");
            var handler = scope?.ServiceProvider.GetRequiredService(concreteType);

            await (Task)concreteType.GetMethod("Handle")?.Invoke(handler, [eventData]);

        }

        public static Subscription Create(Type handlerType, Type eventType) => new(handlerType, eventType);
    }
}
