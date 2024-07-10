using IBM.WMQ;
using IBMMQ.Core.Infra.Abstractions;
using System.Text.Json;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public class IbmMqEventBus : EventBus
    {
        private readonly IbmMqTransportConfiguration _transportConfiguration;
        public IbmMqEventBus(IbmMqTransportConfiguration transportConfiguration, IServiceProvider? provider = null) 
            : base(provider)
        {
            _transportConfiguration = transportConfiguration;
            ConfigureEnvironment(transportConfiguration);
        }

        public override Task Listen<TEvent>()
        {
            while (true)
            {
                using MQQueueManager queueManager = new(_transportConfiguration.QueueManagerName);
                using var queue = queueManager.AccessQueue(_transportConfiguration.QueueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE);
                try
                {
                    var CoaConstants = new int[] { MQC.MQFB_COA, MQC.MQRO_COA, MQC.MQRO_COA_WITH_DATA, MQC.MQRO_COA_WITH_FULL_DATA };
                    var CodConstants = new int[] { MQC.MQFB_COD, MQC.MQRO_COD, MQC.MQRO_COD_WITH_DATA, MQC.MQRO_COD_WITH_FULL_DATA };

                    var mqMessage = new MQMessage();

                    queue.Get(mqMessage, new MQGetMessageOptions
                    {
                        WaitInterval = 500,
                        Options = MQC.MQGMO_WAIT | MQC.MQGMO_SYNCPOINT
                    });

                    var message = string.Empty;
                    if (mqMessage.DataLength > 0)
                        message = mqMessage.ReadString(mqMessage.DataLength);

                    var @event = new TEvent
                    {
                        Payload = message,
                        IsCoa = CoaConstants.Any(coa => coa == mqMessage.Feedback),
                        IsCod = CodConstants.Any(cod => cod == mqMessage.Feedback),
                        MqId = GetHexString(mqMessage.MessageId),
                        CorrelationId = GetHexString(mqMessage.CorrelationId)
                    };

                    var payload = JsonSerializer.Serialize(@event);
                    if (ProcessEvent(typeof(TEvent).Name, payload, _provider).GetAwaiter().GetResult())
                        queueManager.Commit();
                    else
                        queueManager.Backout();
                }
                catch (MQException e)
                {
                    if (e.Reason != MQC.MQRC_NO_MSG_AVAILABLE) throw;
                }
                catch (Exception) { throw; }
            }
        }

        public override void Publish(Event @event)
        {
            // Crie e configure o objeto MQQueueManager
            using MQQueueManager queueManager = new(_transportConfiguration.QueueManagerName);
            using var queue = queueManager.AccessQueue(_transportConfiguration.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
            // Acesse a fila desejada

            // Crie uma mensagem para enviar
            var message = new MQMessage();
            message.WriteString(@event.Payload);

            message.Report = MQC.MQRO_COA | MQC.MQRO_COD | MQC.MQRO_PASS_CORREL_ID;

            //message.MessageType = MQC.MQMT_REQUEST;
            if (!string.IsNullOrWhiteSpace(_transportConfiguration.ReportQueueName))
                //message.ReplyToQueueManagerName = _queueManagerName;
                message.ReplyToQueueName = _transportConfiguration.ReportQueueName;

            // Envie a mensagem para a fila
            queue.Put(message);

        }

        public override Task<string> PublishAsync(Event @event)
        {
            try
            {
                // Crie e configure o objeto MQQueueManager
                using MQQueueManager queueManager = new(_transportConfiguration.QueueManagerName);
                // Acesse a fila desejada
                using var queue = queueManager.AccessQueue(_transportConfiguration.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
                // Crie uma mensagem para enviar
                var message = new MQMessage();
                message.WriteString(@event.Payload);
                message.Report = MQC.MQRO_COA | MQC.MQRO_COD | MQC.MQRO_COPY_MSG_ID_TO_CORREL_ID;

                message.MessageId = Guid.NewGuid().ToByteArray();
                //message.MessageType = MQC.MQMT_REQUEST;
                if (!string.IsNullOrWhiteSpace(_transportConfiguration.ReportQueueName))
                    //message.ReplyToQueueManagerName = _queueManagerName;
                    message.ReplyToQueueName = _transportConfiguration.ReportQueueName;
                // Envie a mensagem para a fila
                queue.Put(message);

                var mqId = GetHexString(message.MessageId);
                return Task.FromResult(mqId);
            }
            catch (MQException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static string GetHexString(byte[] b) => BitConverter.ToString(b).Replace("-", "");
        private static void ConfigureEnvironment(IbmMqTransportConfiguration configuration)
        {
            MQEnvironment.Hostname = configuration.Host;
            MQEnvironment.Port = configuration.Port;
            MQEnvironment.Channel = configuration.ChannelName;
            MQEnvironment.UserId = configuration.UserName;
            MQEnvironment.Password = configuration.Password;
        }
    }
}
