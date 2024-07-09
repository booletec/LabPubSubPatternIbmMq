using IBM.WMQ;
using Ibmmq.Core.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public class IbmMqEventBus : IEventBus
    {
        private readonly EventBusSubscriptionManager _evSubscriptionManager;
        private readonly IbmMqOptions _options;
        private readonly IServiceProvider? _provider;

        public IbmMqEventBus(
            IbmMqOptions options, 
            IServiceProvider? provider = null)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();
            _options = options;
           
            ConfigureEnvironment(options);
            _provider = provider;
        }

        public Task Listen<TEvent>() where TEvent : EventMessage, new()
        {
            while (true)
            {
                using (MQQueueManager queueManager = new(_options.QueueManagerName))
                {
                    using (var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING | MQC.MQOO_INQUIRE))
                    {
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
            }
        }

        public void Publish(Event @event)
        {
            // Crie e configure o objeto MQQueueManager
            using (MQQueueManager queueManager = new(_options.QueueManagerName))
            {
                using (var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING))
                {
                    // Acesse a fila desejada

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
            }
              
        }

       
        public Task<string> PublishAsync(Event @event)
        {

            try
            {
                // Crie e configure o objeto MQQueueManager
                using MQQueueManager queueManager = new(_options.QueueManagerName);
                // Acesse a fila desejada
                using var queue = queueManager.AccessQueue(_options.QueueName, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
                // Crie uma mensagem para enviar
                var message = new MQMessage();
                message.WriteString(@event.Payload);
                message.Report = MQC.MQRO_COA | MQC.MQRO_COD | MQC.MQRO_COPY_MSG_ID_TO_CORREL_ID;

                message.MessageId = Guid.NewGuid().ToByteArray();
                //message.MessageType = MQC.MQMT_REQUEST;
                if (!string.IsNullOrWhiteSpace(_options.ReportQueueName))
                    //message.ReplyToQueueManagerName = _queueManagerName;
                    message.ReplyToQueueName = _options.ReportQueueName;
                // Envie a mensagem para a fila
                queue.Put(message);

                var mqId = GetHexString(message.MessageId);
                return Task.FromResult(mqId);
            }
            catch (MQException mqEx)
            {
                Console.WriteLine($"Erro MQException: {mqEx.ReasonCode} - {mqEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Exception: {ex.Message}");
                throw;
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
                        await subscription.Handle(message, scope);
                }
                processed = true;
            }
            return processed;
        }

        public void Subscribe<TEvent, THandler>()
           where TEvent : Event
           where THandler : IEventHandler<TEvent> => _evSubscriptionManager.AddSubscription<TEvent, THandler>();

        public void Unsubscribe<TEvent, THandler>()
            where TEvent : Event
            where THandler : IEventHandler<TEvent> => _evSubscriptionManager.RemoveSubscription<TEvent, THandler>();


        private static string GetHexString(byte[] b)
        {
            //string result = "";
            //for (int i = 0; i < b.Length; i++)
            //    result += ((b[i] & 0xff) + 0x100).ToString("X2").Substring(1);
            
            //return result;

            return BitConverter.ToString(b).Replace("-", "");
        }

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
    }
}
