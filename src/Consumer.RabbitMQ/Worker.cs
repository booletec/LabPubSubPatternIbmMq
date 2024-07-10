using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;
using IBMMQ.Core.Infra.RabbitMq;

namespace Consumer.RabbitMQ
{
    public class Worker(IServiceProvider provider, ILogger<Worker> logger) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"{nameof(Worker)} started");
            var options = new RabbitMqTransportConfiguration(
                  QueueName: "DEV.QUEUE.1",
                  Host: "localhost",
                  Port: 5672,
                  UserName: "guest",
                  Password: "guest");


            var bus = new RabbitMqEventBus(options, provider);
            bus.Subscribe<ReceivedMessage, MqReceivedHandler>();
            await bus.Listen<ReceivedMessage>();

            await base.StartAsync(cancellationToken);
        }
    }
}
