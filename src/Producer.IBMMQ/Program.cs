using Ibmmq.Core.Conectors;
using Ibmmq.Core.Domain.Events;
using Ibmmq.Core.Domain.Handlers;
using Pubsub.Ibmmq;

var builder = Host.CreateApplicationBuilder(args);
builder
    .Services
    //.AddSingleton<IEventHandler<ReceivedMessage>, MqReceivedHandler>()
    //.AddSingleton<IEventHandler<ReportedMessage>, MqReceivedHandler>()
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();
