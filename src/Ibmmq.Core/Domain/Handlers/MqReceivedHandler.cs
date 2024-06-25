using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;
using System.Text.Json;

namespace Ibmmq.Core.Domain.Handlers
{
    public class MqReceivedHandler : IEventHandler<MqReceivedEvent>
    {
        public Task Handle(MqReceivedEvent @event)
        {
            Console.WriteLine(JsonSerializer.Serialize(@event));

            return Task.CompletedTask;
        }
    }
}
