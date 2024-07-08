using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;

namespace Ibmmq.Core.Domain.Handlers
{
    public class MqReceivedHandler : IEventHandler<EventMessage>
    {
        public async Task Handle(EventMessage @event)
        {

            try
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(@event));

                await Task.FromResult(0);
            }
            catch (Exception)
            {
                throw;
            }
           
        }
    }
}
