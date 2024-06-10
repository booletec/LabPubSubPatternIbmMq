using Ibmmq.Core.Conectors;

namespace Ibmmq.Core.Domain.Events
{
    public class MqReceivedEvent : Event
    {
        public MqReceivedEvent(string? payload) : base(payload)
        {
        }

        public string? MqId { get; set; }
        public string? CorrelationId { get; set; }
    }
}
