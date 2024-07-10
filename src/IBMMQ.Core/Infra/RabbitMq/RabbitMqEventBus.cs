using IBMMQ.Core.Infra.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace IBMMQ.Core.Infra.RabbitMq
{
    public class RabbitMqEventBus : EventBus
    {
        private readonly RabbitMqTransportConfiguration _configuration;
        private readonly IConnection _rabbitMqConnection;
        private readonly IModel _rabbitMqChannel;

        public RabbitMqEventBus(
            RabbitMqTransportConfiguration _configuration,
            IServiceProvider? provider = null) : base(provider)
        {
            this._configuration = _configuration;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host,
                UserName = _configuration.UserName,
                Password = _configuration.Password,
                Port = _configuration.Port
            };

            _rabbitMqConnection = factory.CreateConnection();
            _rabbitMqChannel = _rabbitMqConnection.CreateModel();
        }

        public override Task Listen<TEvent>() 
        {
            return Task.Run(() =>
            {
                _rabbitMqChannel.QueueDeclare(queue: _configuration.QueueName,
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
                        _rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                _rabbitMqChannel.BasicConsume(queue: _configuration.QueueName,
                                             autoAck: false,
                                             consumer: consumer);
            });
        }

        public override void Publish(Event @event)
        {
            var body = Encoding.UTF8.GetBytes(@event.Payload);

            _rabbitMqChannel.BasicPublish(exchange: "",
                                          routingKey: _configuration.QueueName,
                                          basicProperties: null,
                                          body: body);
        }

        public override Task<string> PublishAsync(Event @event)
        {
            Publish(@event);
            return Task.FromResult(@event.Id.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            _evSubscriptionManager.Clear();
            _rabbitMqChannel?.Dispose();
            _rabbitMqConnection?.Dispose();
            base.Dispose(disposing);
        }

    }
}

