using IBMMQ.Core.Infra.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public class IbmMqTransportConfiguration(
                           [Required] string QueueManagerName,
                           [Required] string QueueName,
                           [Required] string ReportQueueName,
                           [Required] string ChannelName,
                           [Required] string Host,
                           [Required] int Port,
                           [Required] string UserName,
                           [Required] string Password) : TransportConfiguration
    {
        public string QueueManagerName { get; } = QueueManagerName;
        public string QueueName { get; } = QueueName;
        public string ReportQueueName { get; } = ReportQueueName;
        public string ChannelName { get; } = ChannelName;
        public string Host { get; } = Host;
        public int Port { get; } = Port;
        public string UserName { get; } = UserName;
        public string Password { get; } = Password;
    }

}
