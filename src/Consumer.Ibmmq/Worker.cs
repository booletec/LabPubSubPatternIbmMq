using Ibmmq.Core.Conectors.Ibmmq;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;

namespace Consumer.Ibmmq
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _provider;

        public Worker(ILogger<Worker> logger, 
            IServiceProvider provider) 
        {
            _logger = logger;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Inicializando o Consumer em: {time}", DateTimeOffset.Now);
            var options = new IbmMqOptions(
                        QueueManagerName: "QM1",
                        QueueName: "DEV.QUEUE.2",
                        ChannelName: "DEV.APP.SVRCONN",
                        Host: "127.0.0.1",
                        Port: 1414,
                        UserName: "app",
                        Password: "passw0rd");

            var bus = new IbmMqEventBus(options, _provider);
            bus.Subscribe<MqReceivedEvent, MqReceivedHandler>();
            bus.StartListener();

            return base.StartAsync(cancellationToken);
        }
    }
}
