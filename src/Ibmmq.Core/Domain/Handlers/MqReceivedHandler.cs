using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ibmmq.Core.Domain.Handlers
{
    public class MqReceivedHandler : 
        IEventHandler<ReceivedMessage>,
        IEventHandler<ReportedMessage>
    {
        private readonly ILogger<MqReceivedHandler> _logguer;

        public MqReceivedHandler(ILogger<MqReceivedHandler> logguer)
        {
            _logguer = logguer;
        }
        public async Task Handle(ReceivedMessage @event)
        {
            try
            {
                _logguer.LogInformation("Mensagem de ENTRADA: {mensagem}", JsonSerializer.Serialize(@event));
                await Task.FromResult(0);
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        public async Task Handle(ReportedMessage @event)
        {
            try
            {
                _logguer.LogInformation("Mensagem de REPORT: {mensagem}", JsonSerializer.Serialize(@event));
                await Task.FromResult(0);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
