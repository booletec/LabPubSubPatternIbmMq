using Ibmmq.Core.Conectors.Ibmmq;
using MassTransit;
using Microsoft.Extensions.Hosting;
using System.Reflection;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                // By default, sagas are in-memory, but should be changed to a durable
                // saga repository.
                x.SetInMemorySagaRepositoryProvider();

                var entryAssembly = Assembly.GetEntryAssembly();

                x.AddConsumers(entryAssembly);
                x.AddSagaStateMachines(entryAssembly);
                x.AddSagas(entryAssembly);
                x.AddActivities(entryAssembly);

                // Rabbit MQ
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });

                // Amazon SQS
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host("us-east-1", h =>
                    {
                        h.AccessKey("your-iam-access-key");
                        h.SecretKey("your-iam-secret-key");
                    });

                    cfg.ConfigureEndpoints(context);
                });

                // In Memory
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });

                // IBM MQ
                x.UsingIbmMq((context, cfg) =>
                {
                    cfg.Host(h =>
                    {
                        h.Channel("DEV.APP.SVRCONN");
                        h.HostName("127.0.0.1");
                        h.Port(1414);
                        h.UserId("app");
                        h.Password("passw0rd");

                        h.Config(new IbmMqOptions(
                            QueueManagerName: "QM1",
                            QueueName: "DEV.QUEUE.2",
                            ChannelName: "DEV.APP.SVRCONN",
                            Host: "127.0.0.1",
                            Port: 1414,
                            UserName: "app",
                            Password: "passw0rd"));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        });
