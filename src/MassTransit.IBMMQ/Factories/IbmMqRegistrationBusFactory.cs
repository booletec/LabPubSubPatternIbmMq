using MassTransit.Configuration;
using MassTransit.Transports;

namespace MassTransit.IBMMQ.Factories;

public class IbmMqRegistrationBusFactory : TransportRegistrationBusFactory<IIbmMqReceiveEndpointConfigurator>
{
    private readonly Action<IBusRegistrationContext, IIbmMqBusFactoryConfigurator> _configure;

    public IbmMqRegistrationBusFactory(Action<IBusRegistrationContext, IIbmMqBusFactoryConfigurator> configure)
        : base(null)
    {
        _configure = configure;
    }

    public override IBusInstance CreateBus(
        IBusRegistrationContext context,
        IEnumerable<IBusInstanceSpecification> specifications,
        string busName)
    {
        throw new NotImplementedException();
    }
}


public interface IIbmMqReceiveEndpointConfigurator : IReceiveEndpointConfigurator
{

}
