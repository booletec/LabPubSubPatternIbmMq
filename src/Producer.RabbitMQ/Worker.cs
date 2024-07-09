using Ibmmq.Core.Domain.Events;
using IBMMQ.Core.Infra.RabbitMq;

namespace Producer.RabbitMQ
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var options = new RabbitMqOptions(
                   QueueName: "DEV.QUEUE.1",
                   Host: "localhost",
                   Port: 5672,
                   UserName: "guest",
                   Password: "guest");

                var bus = new RabbitMqEventBus(options);
                var messageId = await bus.PublishAsync(new EventMessage { Payload = "Payment done!" });
                _logger.LogInformation("Mensagem publicada em: {time} com id {messageId}", DateTimeOffset.Now, messageId);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
