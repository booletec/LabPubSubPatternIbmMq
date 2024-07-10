using Ibmmq.Core.Domain.Events;
using IBMMQ.Core.Infra.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ibmmq.Core.Domain.Handlers;

public class MqReceivedHandler(ILogger<MqReceivedHandler> logguer) : 
    IEventHandler<ReceivedMessage>,
    IEventHandler<ReportedMessage>
{
  
    public async Task Handle(ReceivedMessage @event)
    {
        try
        {
            logguer.LogInformation("Mensagem de ENTRADA: {mensagem}", JsonSerializer.Serialize(@event));
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
            logguer.LogInformation("Mensagem de REPORT: {mensagem}", JsonSerializer.Serialize(@event));
            await Task.FromResult(0);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
