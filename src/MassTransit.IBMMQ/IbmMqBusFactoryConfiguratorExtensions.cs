using Ibmmq.Core.Conectors.Ibmmq;
using MassTransit.IBMMQ.Factories;

namespace MassTransit;

public static class IbmMqBusFactoryConfiguratorExtensions
{
    /// <summary>
    /// Configure MassTransit to use RabbitMQ for the transport.
    /// </summary>
    /// <param name="configurator">The registration configurator (configured via AddMassTransit)</param>
    /// <param name="configure">The configuration callback for the bus factory</param>
    public static void UsingIbmMq(
        this IBusRegistrationConfigurator configurator,
        Action<IBusRegistrationContext, IIbmMqBusFactoryConfigurator>? configure = null)
    {
        configurator.SetBusFactory(new IbmMqRegistrationBusFactory(configure));
    }
}

public interface IIbmMqBusFactoryConfigurator :
    IBusFactoryConfigurator<IIbmMqReceiveEndpointConfigurator>
{
    new object PublishTopology { get; }

    void Host(Action<IIbmMqHostConfigurator> configure);
}

public interface IIbmMqHostConfigurator
{
    void HostName(string host);
    void Port(int port);
    void Channel(string channel);
    void UserId(string userId);
    void Password(string password);
    void Config(IbmMqTransportConfiguration config);
}