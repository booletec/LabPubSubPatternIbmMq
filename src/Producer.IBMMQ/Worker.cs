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
                       QueueName: "DEV.QUEUE.1",
                       ReportQueueName: "DEV.QUEUE.2",
                       ChannelName: "DEV.APP.SVRCONN",
                       Host: "127.0.0.1",
                       Port: 1414,
                       UserName: "app",
                       Password: "passw0rd");

                    var bus = new IbmMqEventBus(options);
                    var messageId = await bus.PublishAsync(new EventMessage("Payment done!"));


                    _logger.LogInformation("Mensagem publicada em: {time} com id {messageId}", DateTimeOffset.Now, messageId);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }


            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    // Configurações de conexão
            //    string hostName = "127.0.0.1";
            //    int port = 1414;
            //    string channel = "DEV.APP.SVRCONN";
            //    string _queueManager = "QM1";
            //    string requestQueueName = "DEV.QUEUE.1";
            //    string responseQueueName = "DEV.QUEUE.2";
            //    string coaQueueName = "DEV.QUEUE.3";
            //    string userId = "app";
            //    string password = "passw0rd";

            //    // Configurar as opções de conexão
            //    MQEnvironment.Hostname = hostName;
            //    MQEnvironment.Port = port;
            //    MQEnvironment.Channel = channel;
            //    MQEnvironment.UserId = userId;
            //    MQEnvironment.Password = password;

            //    try
            //    {
            //        // Conectar ao queue manager
            //        MQQueueManager queueManager = new MQQueueManager(_queueManager);

            //        // Acessar as filas
            //        MQQueue requestQueue = queueManager.AccessQueue(requestQueueName, MQC.MQOO_OUTPUT);
            //        MQQueue responseQueue = queueManager.AccessQueue(responseQueueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_OUTPUT);
            //        MQQueue coaQueue = queueManager.AccessQueue(coaQueueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_OUTPUT);

            //        // Criar a mensagem de pedido
            //        MQMessage requestMessage = new MQMessage();
            //        requestMessage.WriteString("Mensagem de teste");

            //        // Configurar a mensagem para solicitar COA
            //        requestMessage.Report = MQC.MQRO_COA;

            //        // Definir a fila de resposta
            //        requestMessage.ReplyToQueueName = responseQueueName;

            //        // Criar o objeto de mensagem
            //        MQPutMessageOptions pmo = new MQPutMessageOptions();

            //        // Enviar a mensagem
            //        requestQueue.Put(requestMessage, pmo);
            //        Console.WriteLine("Mensagem de pedido enviada.");

            //        // Receber a confirmação de chegada (COA)
            //        MQMessage coaMessage = new MQMessage();
            //        MQGetMessageOptions gmo = new MQGetMessageOptions();
            //        gmo.Options = MQC.MQGMO_WAIT | MQC.MQGMO_CONVERT;
            //        gmo.WaitInterval = 11000; // Espera até 5 segundos por uma COA

            //        try
            //        {
            //            coaQueue.Get(coaMessage, gmo);
            //            string coaText = coaMessage.ReadString(coaMessage.MessageLength);
            //            Console.WriteLine("Mensagem COA recebida: " + coaText);
            //        }
            //        catch (MQException ex)
            //        {
            //            if (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
            //            {
            //                Console.WriteLine("Nenhuma mensagem COA recebida.");
            //            }
            //            else
            //            {
            //                throw;
            //            }
            //        }

            //        // Receber a mensagem de resposta
            //        MQMessage responseMessage = new MQMessage();
            //        gmo.WaitInterval = 11000; // Espera até 5 segundos por uma resposta

            //        try
            //        {
            //            responseQueue.Get(responseMessage, gmo);
            //            string responseText = responseMessage.ReadString(responseMessage.MessageLength);
            //            Console.WriteLine("Mensagem de resposta recebida: " + responseText);
            //        }
            //        catch (MQException ex)
            //        {
            //            if (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
            //            {
            //                Console.WriteLine("Nenhuma mensagem de resposta recebida.");
            //            }
            //            else
            //            {
            //                throw;
            //            }
            //        }

            //        // Fechar recursos
            //        requestQueue.Close();
            //        responseQueue.Close();
            //        coaQueue.Close();
            //        queueManager.Disconnect();
            //    }
            //    catch (MQException mqe)
            //    {
            //        Console.WriteLine("MQException: " + mqe.Message);
            //        Console.WriteLine("MQ Reason Code: " + mqe.ReasonCode);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("Exception: " + e.Message);
            //    }
            //}
        }

    }
}
