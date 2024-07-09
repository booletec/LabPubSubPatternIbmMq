using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace IBMMQ.Core.Infra.RabbitMq
{
    public class RabbitMqEventBus : IEventBus, IDisposable
    {
        private readonly EventBusSubscriptionManager _evSubscriptionManager;
        private readonly RabbitMqOptions _options;
        private readonly IServiceProvider? _provider;
        private readonly IConnection _rabbitMqConnection;
        private readonly IModel _rabbitMqChannel;

        public RabbitMqEventBus(
            RabbitMqOptions options,
            IServiceProvider? provider = null)
        {
            _evSubscriptionManager = new EventBusSubscriptionManager();
            _options = options;
            _provider = provider;

            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                UserName = options.UserName,
                Password = options.Password,
                Port = options.Port
            };

            _rabbitMqConnection = factory.CreateConnection();
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();
        }

        public Task Listen<TEvent>() where TEvent : EventMessage, new()
        {
            return Task.Run(() =>
            {
                _rabbitMqChannel.QueueDeclare(queue: _options.QueueName,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                var consumer = new EventingBasicConsumer(_rabbitMqChannel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var @event = new TEvent
                    {
                        Payload = message
                    };

                    var payload = JsonSerializer.Serialize(@event);
                    if (await ProcessEvent(typeof(TEvent).Name, payload, _provider))
                    {
                        _rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                _rabbitMqChannel.BasicConsume(queue: _options.QueueName,
                                             autoAck: false,
                                             consumer: consumer);
            });
        }

        public void Publish(Event @event)
        {
            var body = Encoding.UTF8.GetBytes(@event.Payload);

            _rabbitMqChannel.BasicPublish(exchange: "",
                                          routingKey: _options.QueueName,
                                          basicProperties: null,
                                          body: body);
        }

        public Task<string> PublishAsync(Event @event)
        {
            Publish(@event);
            return Task.FromResult(Guid.NewGuid().ToString());
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _evSubscriptionManager.Clear();
            _rabbitMqChannel?.Dispose();
            _rabbitMqConnection?.Dispose();
        }

    }
}

