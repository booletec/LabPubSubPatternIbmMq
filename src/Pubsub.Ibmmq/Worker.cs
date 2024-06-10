using Ibmmq.Core.Conectors.Ibmmq;
using Ibmmq.Core.Domain.Events;

namespace Pubsub.Ibmmq
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
            _logger.LogInformation("Inicializando o PRODUTOR em: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    var options = new IbmMqOptions(
                       QueueManagerName: "QM1",
                       QueueName: "DEV.QUEUE.2",
                       ChannelName: "DEV.APP.SVRCONN",
                       Host: "127.0.0.1",
                       Port: 1414,
                       UserName: "app",
                       Password: "passw0rd");

                    var bus = new IbmMqEventBus(options);
                    bus.Publish(new MqReceivedEvent("Payment done!"));

                   
                    _logger.LogInformation("Mensagem publicada em: {time}", DateTimeOffset.Now);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }
}
