using Consumer.Ibmmq;
using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;

var builder = Host.CreateApplicationBuilder(args);
builder
    .Services
    .AddSingleton<IEventHandler<MqReceivedEvent>, MqReceivedHandler>()
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();
