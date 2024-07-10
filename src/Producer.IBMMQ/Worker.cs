using Ibmmq.Core.Conectors.Ibmmq;
using IBMMQ.Core.Infra.Abstractions;

namespace Pubsub.Ibmmq;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inicializando o PRODUTOR em: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var options = new IbmMqTransportConfiguration(
                   QueueManagerName: "QM1",
                   QueueName: "DEV.QUEUE.1",
                   ReportQueueName: "DEV.QUEUE.2",
                   ChannelName: "DEV.APP.SVRCONN",
                   Host: "127.0.0.1",
                   Port: 1414,
                   UserName: "app",
                   Password: "passw0rd");

                var bus = new IbmMqEventBus(options);
                var messageId = await bus.PublishAsync(new Event { Payload = "Payment done!" });
                logger.LogInformation("Mensagem publicada em: {time} com id {messageId}", DateTimeOffset.Now, messageId);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

}

