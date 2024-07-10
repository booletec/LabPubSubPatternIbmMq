using IBMMQ.Core.Infra.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace IBMMQ.Core.Infra.RabbitMq
{
    public class RabbitMqTransportConfiguration([Required] string QueueName,
                              [Required] string Host,
                              [Required] int Port,
                              [Required] string UserName,
                              [Required] string Password) : TransportConfiguration
    {
        public string QueueName { get; } = QueueName;
        public string Host { get; } = Host;
        public int Port { get; } = Port;
        public string UserName { get; } = UserName;
        public string Password { get; } = Password;
    }
}
