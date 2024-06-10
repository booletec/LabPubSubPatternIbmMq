using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;

namespace Ibmmq.Core.Domain.Handlers
{
    public class MqReceivedHandler : IEventHandler<MqReceivedEvent>
    {
        public async Task Handle(MqReceivedEvent @event)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(@event));

            await Task.FromResult(0);
        }
    }
}
