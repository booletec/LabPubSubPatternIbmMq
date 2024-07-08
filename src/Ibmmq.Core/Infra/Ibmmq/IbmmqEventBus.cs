using IBM.WMQ;
using Ibmmq.Core.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public class IbmMqEventBus : IEventBus
    {
        private readonly EventBusSubscriptionManager _evSubscriptionManager;
        private readonly IbmMqOptions _options;
        private readonly IServiceProvider? _provider;

        public IbmMqEventBus(
            IbmMqOptions options, 
            IServiceProvider provider)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();
            _options = options;
           
            ConfigureEnvironment(options);
            _provider = provider;
        }

        public IbmMqEventBus(IbmMqOptions options)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();
            _options = options;
            ConfigureEnvironment(options);
        }

        public void StartListener()
        {
            while (true)
            {
                using MQQueueManager queueManager = new(_options.QueueManagerName);
                
                using var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE);
                try
                {
                    var mqMessage = new MQMessage();

                    queue.Get(mqMessage, new MQGetMessageOptions
                    {
                        WaitInterval = 500,
                        Options = MQC.MQGMO_WAIT | MQC.MQGMO_SYNCPOINT | MQC.MQGMO_CONVERT
                    });

                    var message = string.Empty;
                    if (mqMessage.DataLength > 0)
                        message = mqMessage.ReadString(mqMessage.DataLength);

                    var @event = new EventMessage(message)
                    {
                        IsCoa = mqMessage.Feedback == (MQC.MQFB_COA | MQC.MQRO_COA | MQC.MQRO_COA_WITH_DATA | MQC.MQRO_COA_WITH_FULL_DATA),
                        IsCod = mqMessage.Feedback == (MQC.MQFB_COD | MQC.MQRO_COD | MQC.MQRO_COD_WITH_DATA | MQC.MQRO_COD_WITH_FULL_DATA),

                        MqId = BitConverter.ToString(mqMessage.MessageId),
                        CorrelationId = BitConverter.ToString(mqMessage.CorrelationId)
                    };

                    if (ProcessEvent(nameof(EventMessage), System.Text.Json.JsonSerializer.Serialize(@event), _provider).GetAwaiter().GetResult())
                        queueManager.Commit();

                }
                catch (MQException e)
                {
                    if (e.Reason != MQC.MQRC_NO_MSG_AVAILABLE) throw;
                }
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message, IServiceProvider? _provider)
        {
            var processed = false;

            if (_evSubscriptionManager.HasSubscriptions(eventName))
            {
                using (var scope = _provider?.CreateAsyncScope())
                {
                    var subscriptions = _evSubscriptionManager.GetHandlers(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        await subscription.Handle(message, scope);
                    }
                }

                processed = true;
            }
            return processed;
        }

        public void Publish(Event @event)
        {
            // Crie e configure o objeto MQQueueManager
            using MQQueueManager queueManager = new(_options.QueueManagerName);
            // Acesse a fila desejada
            using var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
            // Crie uma mensagem para enviar
            var message = new MQMessage();
            message.WriteString(@event.Payload);

            message.Report = MQC.MQRO_COA | MQC.MQRO_COD | MQC.MQRO_PASS_CORREL_ID;

            //message.MessageType = MQC.MQMT_REQUEST;
            if (!string.IsNullOrWhiteSpace(_options.ReportQueueName))
                //message.ReplyToQueueManagerName = _queueManagerName;
                 message.ReplyToQueueName = _options.ReportQueueName;

            // Envie a mensagem para a fila
            queue.Put(message);
        }

        public void Subscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent> => _evSubscriptionManager.AddSubscription<TEvent, THandler>();

        public void Unsubscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent> => _evSubscriptionManager.RemoveSubscription<TEvent, THandler>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) => _evSubscriptionManager.Clear();

        private static void ConfigureEnvironment(IbmMqOptions options)
        {
            MQEnvironment.Hostname = options.Host;
            MQEnvironment.Port = options.Port;
            MQEnvironment.Channel = options.ChannelName;
            MQEnvironment.UserId = options.UserName;
            MQEnvironment.Password = options.Password;
        }

        public Task<string> PublishAsync(Event @event)
        {
            // Crie e configure o objeto MQQueueManager
            using MQQueueManager queueManager = new(_options.QueueManagerName);
            // Acesse a fila desejada
            using var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
            // Crie uma mensagem para enviar
            var message = new MQMessage();
            message.WriteString(@event.Payload);
            message.Report = MQC.MQRO_COA | MQC.MQRO_COD | MQC.MQRO_PASS_CORREL_ID ;

            //message.MessageType = MQC.MQMT_REQUEST;
            if (!string.IsNullOrWhiteSpace(_options.ReportQueueName))
                //message.ReplyToQueueManagerName = _queueManagerName;
                message.ReplyToQueueName = _options.ReportQueueName;
            // Envie a mensagem para a fila
            queue.Put(message);

            var mqId = GetHexString(message.MessageId);
            return Task.FromResult(mqId);

        }

        private static string GetHexString(byte[] b)
        {
            string result = "";
            for (int i = 0; i < b.Length; i++)
            {
                result += ((b[i] & 0xff) + 0x100).ToString("X2").Substring(1);
            }
            return result;
        }

    }
}
