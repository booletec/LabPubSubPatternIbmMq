using Ibmmq.Core.Conectors.Ibmmq;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;

namespace Consumer.Ibmmq;

public class Worker(IServiceProvider provider, ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Inicializando o CONSUMIDOR em: {time}", DateTimeOffset.Now);

        // Configurações para o primeiro listener da fila de entrada
        var options = new IbmMqTransportConfiguration(
            QueueManagerName: "QM1",
            QueueName: "DEV.QUEUE.1",
            ReportQueueName: "DEV.QUEUE.2",
            ChannelName: "DEV.APP.SVRCONN",
            Host: "127.0.0.1",
            Port: 1414,
            UserName: "app",
            Password: "passw0rd");

        var bus = new IbmMqEventBus(options, provider);
        bus.Subscribe<ReceivedMessage, MqReceivedHandler>();

        // Configurações para o segundo listener da fila de respostas.
        var optionsCOA = new IbmMqTransportConfiguration(
            QueueManagerName: "QM1",
            QueueName: "DEV.QUEUE.2",
            ReportQueueName: "DEV.QUEUE.2",
            ChannelName: "DEV.APP.SVRCONN",
            Host: "127.0.0.1",
            Port: 1414,
            UserName: "app",
            Password: "passw0rd");

        var busCOA = new IbmMqEventBus(optionsCOA, provider);
        busCOA.Subscribe<ReportedMessage, MqReceivedHandler>();

        // Inicializa ambos os listeners em paralelo e loga mensagens apropriadas
        var listenForReceivedMessagesQueue = Task.Run(async () =>
        {
            await bus.Listen<ReceivedMessage>();
            logger.LogInformation("INICIANDO SUBSCRIBE PARA A QUEUE DE ENTRADA EM {time}", DateTimeOffset.Now);
        }, cancellationToken);

        var listenForReportedMessagesQueue = Task.Run(async () =>
        {
            await busCOA.Listen<ReportedMessage>();
            logger.LogInformation("INICIANDO SUBSCRIBE PARA A QUEUE DE REPORT EM {time}", DateTimeOffset.Now);
        }, cancellationToken);

        try
        {
            // Aguarda ambos os listeners serem iniciados
            await Task.WhenAll(listenForReceivedMessagesQueue, listenForReportedMessagesQueue);
        }
        catch (Exception ex)
        {
            logger.LogError("Erro ao iniciar os listeners: {message}", ex.Message);
            throw;
        }

        // Aguarda a base StartAsync
        await base.StartAsync(cancellationToken);
    }

}
