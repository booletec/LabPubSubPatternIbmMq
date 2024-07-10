using IBMMQ.Core.Infra.Abstractions;
using IBMMQ.Core.Infra.RabbitMq;

namespace Producer.RabbitMQ
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var options = new RabbitMqTransportConfiguration(
                   QueueName: "DEV.QUEUE.1",
                   Host: "localhost",
                   Port: 5672,
                   UserName: "guest",
                   Password: "guest");

                var bus = new RabbitMqEventBus(options);
                var messageId = await bus.PublishAsync(new Event { Payload = "Payment done!" });
                logger.LogInformation("Mensagem publicada em: {time} com id {messageId}", DateTimeOffset.Now, messageId);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
