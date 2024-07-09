using System.ComponentModel.DataAnnotations;

namespace IBMMQ.Core.Infra.RabbitMq
{
    public record RabbitMqOptions([Required] string QueueName,
                              [Required] string Host,
                              [Required] int Port,
                              [Required] string UserName,
                              [Required] string Password);
}
