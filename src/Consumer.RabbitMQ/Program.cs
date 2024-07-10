using Consumer.RabbitMQ;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;
using IBMMQ.Core.Infra.Abstractions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddSingleton<IEventHandler<ReceivedMessage>, MqReceivedHandler>()
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();
