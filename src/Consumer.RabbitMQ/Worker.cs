using Ibmmq.Core.Conectors.Ibmmq;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;
using IBMMQ.Core.Infra.RabbitMq;
using Microsoft.Extensions.Options;

namespace Consumer.RabbitMQ
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider provider, ILogger<Worker> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {

            var options = new RabbitMqOptions(
                  QueueName: "DEV.QUEUE.1",
                  Host: "localhost",
                  Port: 5672,
                  UserName: "guest",
                  Password: "guest");


            var bus = new RabbitMqEventBus(options, _provider);
            bus.Subscribe<ReceivedMessage, MqReceivedHandler>();
            await bus.Listen<ReceivedMessage>();

            await base.StartAsync(cancellationToken);
        }
    }
}
