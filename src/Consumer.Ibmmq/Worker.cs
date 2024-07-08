using Ibmmq.Core.Conectors.Ibmmq;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;

namespace Consumer.Ibmmq
{
    public class Worker(ILogger<Worker> logger,
        IServiceProvider provider) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IServiceProvider _provider = provider;

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override Task StartAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Inicializando o Consumer em: {time}", DateTimeOffset.Now);
            var options = new IbmMqOptions(
                        QueueManagerName: "QM1",
                        QueueName: "DEV.QUEUE.2",
                        //ReportQueueName: "DEV.QUEUE.2",
                        ChannelName: "DEV.APP.SVRCONN",
                        Host: "127.0.0.1",
                        Port: 1414,
                        UserName: "app",
                        Password: "passw0rd");

            var bus = new IbmMqEventBus(options, _provider);
            bus.Subscribe<EventMessage, MqReceivedHandler>();
            bus.StartListener();

            //var optionsCOA = new IbmMqOptions(
            //           QueueManagerName: "QM1",
            //           QueueName: "DEV.QUEUE.2",
            //           ReportQueueName: "DEV.QUEUE.2",
            //           ChannelName: "DEV.APP.SVRCONN",
            //           Host: "127.0.0.1",
            //           Port: 1414,
            //           UserName: "app",
            //           Password: "passw0rd");

            //var busCOA = new IbmMqEventBus(optionsCOA, _provider);
            //busCOA.Subscribe<MqReceivedEvent, MqReceivedHandler>();
            //busCOA.StartListener();

            return base.StartAsync(cancellationToken);
        }
    }
}
