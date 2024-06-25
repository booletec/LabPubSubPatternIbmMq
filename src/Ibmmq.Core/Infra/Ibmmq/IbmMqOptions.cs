using System.ComponentModel.DataAnnotations;

namespace Ibmmq.Core.Conectors.Ibmmq
{
    public record IbmMqOptions([Required] string QueueManagerName,
                               [Required] string QueueName,
                               [Required] string ChannelName,
                               [Required] string Host,
                               [Required] int Port,
                               [Required] string UserName,
                               [Required] string Password);
}
