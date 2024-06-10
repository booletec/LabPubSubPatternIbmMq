using IBM.WMQ;
using Ibmmq.Core.Domain.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public class IbmMqEventBus : IEventBus
    {
        private readonly string _queueManagerName;
        private readonly string _queueName;
        private readonly EventBusSubscriptionManager _evSubscriptionManager;
        private IServiceProvider? _provider;

        public IbmMqEventBus(IbmMqOptions options, IServiceProvider provider)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();

            _queueManagerName = options.QueueManagerName;
            _queueName = options.QueueName;

            ConfigureEnvironment(options);
            _provider = provider;

        }

        public IbmMqEventBus(IbmMqOptions options)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();

            _queueManagerName = options.QueueManagerName;
            _queueName = options.QueueName;

            ConfigureEnvironment(options);

        }

        public void StartListener()
        {

            while (true)
            {
                using MQQueueManager queueManager = new(_queueManagerName);
                using (var queue = queueManager.AccessQueue(_queueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING))
                {
                    try
                    {
                        var mqMessage = new MQMessage();

                        queue.Get(mqMessage, new MQGetMessageOptions
                        {
                            WaitInterval = 500,
                            Options = MQC.MQGMO_WAIT + MQC.MQGMO_SYNCPOINT
                        });

                        var message = mqMessage.ReadString(mqMessage.DataLength);

                        var @event = new MqReceivedEvent(message)
                        {
                            MqId = System.Text.Encoding.Default.GetString(mqMessage.MessageId),
                            CorrelationId = System.Text.Encoding.Default.GetString(mqMessage.CorrelationId)
                        };

                        if (ProcessEvent(nameof(MqReceivedEvent), System.Text.Json.JsonSerializer.Serialize(@event)).GetAwaiter().GetResult())
                            queueManager.Commit();

                    }
                    catch (MQException e)
                    {
                        if (e.Reason != 2033) throw;
                    }
                   
                }
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            var processed = false;

            if (_evSubscriptionManager.HasSubscriptions(eventName))
            {

                using (var scope = _provider.CreateAsyncScope())
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
            using MQQueueManager queueManager = new(_queueManagerName);
            // Acesse a fila desejada
            using (var queue = queueManager.AccessQueue(_queueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING))
            {
                // Crie uma mensagem para enviar
                var message = new MQMessage();
                message.WriteString(@event.Payload);

                // Envie a mensagem para a fila
                queue.Put(message);
            }
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

        private void ConfigureEnvironment(IbmMqOptions options)
        {
            MQEnvironment.Hostname = options.Host;
            MQEnvironment.Port = options.Port;
            MQEnvironment.Channel = options.ChannelName;
            MQEnvironment.UserId = options.UserName;
            MQEnvironment.Password = options.Password;

        }

    }
}
